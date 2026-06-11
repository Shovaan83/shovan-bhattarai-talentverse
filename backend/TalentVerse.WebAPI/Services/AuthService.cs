using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Account;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailService _emailService;
    private readonly ICreditService _creditService;
    private readonly IBadgeService _badgeService;
    public readonly ILogger<AuthService> _logger;

    private const int UsernameMinLength = 3;
    private const int UsernameMaxLength = 50;
    private const int BioMaxLength = 500;
    private const int VerificationCodeLength = 6;
    private static readonly Regex UsernameRegex = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            var addr = new MailAddress(email);
            return string.Equals(addr.Address, email, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static string BuildUsernameSeed(string? preferredName, string email)
    {
        var seed = !string.IsNullOrWhiteSpace(preferredName)
            ? preferredName.Trim()
            : email.Split('@')[0];

        seed = Regex.Replace(seed.ToLowerInvariant(), @"[^a-z0-9_]+", "_");
        seed = Regex.Replace(seed, "_+", "_").Trim('_');

        if (seed.Length < UsernameMinLength)
            seed = $"user_{seed}".TrimEnd('_');

        if (seed.Length < UsernameMinLength)
            seed = "user";

        return seed.Length > UsernameMaxLength
            ? seed[..UsernameMaxLength]
            : seed;
    }

    private async Task<string> GenerateUniqueUsernameAsync(
        string? preferredName,
        string email,
        string? excludedUserId = null)
    {
        var baseUsername = BuildUsernameSeed(preferredName, email);
        var candidate = baseUsername;
        var suffix = 1;

        while (await _userManager.Users.AnyAsync(u =>
            u.NormalizedUserName == _userManager.NormalizeName(candidate) &&
            (excludedUserId == null || u.Id != excludedUserId)))
        {
            var suffixText = suffix.ToString();
            var maxBaseLength = Math.Max(UsernameMinLength, UsernameMaxLength - suffixText.Length);
            var truncatedBase = baseUsername.Length > maxBaseLength
                ? baseUsername[..maxBaseLength]
                : baseUsername;

            candidate = $"{truncatedBase}{suffixText}";
            suffix++;
        }

        return candidate;
    }

    private static bool IsEmailUsername(AppUser user) =>
        !string.IsNullOrWhiteSpace(user.UserName) &&
        !string.IsNullOrWhiteSpace(user.Email) &&
        string.Equals(user.UserName, user.Email, StringComparison.OrdinalIgnoreCase);

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService, 
        ITwoFactorService twoFactorService,
        IEmailService emailService,
        ICreditService creditService,
        IBadgeService badgeService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
        _emailService = emailService;
        _creditService = creditService;
        _badgeService = badgeService;
        _logger = logger;
    }

    public async Task<ServiceResponse<UserDto>> RegisterAsync(RegisterDto registerDto, string ipAddress = "Unknown")
    {
        try
        {
            // Guard clause: null request
            if (registerDto is null)
                return ServiceResponse<UserDto>.FailureResponse("Registration payload is required.");

            // Input sanitization
            var trimmedEmail = registerDto.Email?.Trim();
            var trimmedUsername = registerDto.Username?.Trim();
            var password = registerDto.Password; // Do not trim passwords
            var trimmedBio = registerDto.Bio?.Trim();

            // Fail-fast: cheap validations first
            if (string.IsNullOrWhiteSpace(trimmedUsername))
                return ServiceResponse<UserDto>.FailureResponse("Username is required.");

            if (trimmedUsername.Length < UsernameMinLength || trimmedUsername.Length > UsernameMaxLength)
                return ServiceResponse<UserDto>.FailureResponse($"Username must be between {UsernameMinLength} and {UsernameMaxLength} characters.");

            if (!UsernameRegex.IsMatch(trimmedUsername))
                return ServiceResponse<UserDto>.FailureResponse("Username can only contain letters, numbers, and underscores.");

            if (string.IsNullOrWhiteSpace(trimmedEmail))
                return ServiceResponse<UserDto>.FailureResponse("Email is required.");

            if (!IsValidEmail(trimmedEmail))
                return ServiceResponse<UserDto>.FailureResponse("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(password))
                return ServiceResponse<UserDto>.FailureResponse("Password is required.");

            if (!string.IsNullOrEmpty(trimmedBio) && trimmedBio.Length > BioMaxLength)
                return ServiceResponse<UserDto>.FailureResponse($"Bio cannot exceed {BioMaxLength} characters.");

            // Business rule validation: uniqueness (expensive)
            var normalizedEmail = _userManager.NormalizeEmail(trimmedEmail);
            if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
                return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.UserExists);

            var normalizedUsername = _userManager.NormalizeName(trimmedUsername);
            if (await _userManager.Users.AnyAsync(u => u.NormalizedUserName == normalizedUsername))
                return ServiceResponse<UserDto>.FailureResponse("Username is already taken.");

            // Execute operation
            var appUser = new AppUser
            {
                UserName = trimmedUsername,
                Email = trimmedEmail,
                Bio = string.IsNullOrEmpty(registerDto.Bio) ? null : trimmedBio,
                IsTwoFactorSetupComplete = false // Email/password users need to setup 2FA
            };

            // Proper framework API usage: create user (also sets normalized fields)
            var createdUser = await _userManager.CreateAsync(appUser, password);
            if (!createdUser.Succeeded)
            {
                var errors = createdUser.Errors.Select(e => e.Description).ToList();
                return ServiceResponse<UserDto>.FailureResponse("User creation failed.", errors);
            }

            // Execute operation: role assignment
            var addRoleResult = await _userManager.AddToRoleAsync(appUser, AppConstant.Roles.Member);
            if (!addRoleResult.Succeeded)
            {
                var errors = addRoleResult.Errors.Select(e => e.Description).ToList();
                return ServiceResponse<UserDto>.FailureResponse("User created but role assignment failed.", errors);
            }

            // Award signup bonus credits and Welcome Aboard badge
            await _creditService.AwardSignupBonusAsync(appUser.Id);
            await _badgeService.EvaluateOnSignupAsync(appUser.Id);

            var tokenPair = await _tokenService.GenerateTokenPairAsync(appUser, ipAddress);

            return ServiceResponse<UserDto>.SuccessResponse(
                new UserDto
                {
                    Username = appUser.UserName,
                    Email = appUser.Email,
                    Bio = appUser.Bio,
                    Token = tokenPair.AccessToken,
                    IsProfileComplete = appUser.IsProfileComplete,
                    IsTwoFactorSetupComplete = appUser.IsTwoFactorSetupComplete,
                    HasPassword = true, // Email/password registration always has password
                    ProfilePictureUrl = appUser.ProfilePictureURL,
                    Location = appUser.Location,
                    GitHubUrl = appUser.GitHubUrl,
                    LinkedInUrl = appUser.LinkedInUrl,
                    TwitterUrl = appUser.TwitterUrl,
                    WebsiteUrl = appUser.WebsiteUrl
                },
                AppConstant.SuccessMessages.RegistrationSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for email {Email}", nameof(RegisterAsync), registerDto?.Email);
            return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<UserDto>> LoginAsync(LoginDto loginDto, string ipAddress)
    {
        try
        {
            // Guard clause: null request
            if (loginDto is null)
                return ServiceResponse<UserDto>.FailureResponse("Login payload is required.");

            // Input sanitization
            var trimmedEmail = loginDto.Email?.Trim();
            var password = loginDto.Password; // Do not trim passwords

            // Fail-fast: cheap validations
            if (string.IsNullOrWhiteSpace(trimmedEmail))
                return ServiceResponse<UserDto>.FailureResponse("Email is required.");

            if (!IsValidEmail(trimmedEmail))
                return ServiceResponse<UserDto>.FailureResponse("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(password))
                return ServiceResponse<UserDto>.FailureResponse("Password is required.");

            // Expensive operation: lookup by normalized email
            var normalizedEmail = _userManager.NormalizeEmail(trimmedEmail);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
            if (user == null)
                return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.InvalidLogin);

            // Security: enforce lockout if configured
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var minutesRemaining = lockoutEnd.HasValue
                    ? Math.Max(1, (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                    : 0;

                _logger.LogWarning("Locked out user {UserId} attempted login", user.Id);
                return ServiceResponse<UserDto>.FailureResponse(
                    $"Account is locked due to multiple failed login attempts. Please try again in {minutesRemaining} minute(s).");
            }

            // Expensive operation: password verification
            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                // Record failed attempt (and potentially trigger lockout)
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("User {UserId} locked out after failed login", user.Id);
                    return ServiceResponse<UserDto>.FailureResponse(
                        "Account has been locked due to multiple failed login attempts. Please try again later.");
                }

                return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.InvalidLogin);
            }

            // Reset failed count on a successful password check
            await _userManager.ResetAccessFailedCountAsync(user);

            // Business rule: require confirmed email if enabled
            if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            {
                return ServiceResponse<UserDto>.FailureResponse(
                    "Please confirm your email address before logging in. Check your inbox for the confirmation link.");
            }

            if (user.TwoFactorEnabled)
            {
                // Execute operation: generate + store 2FA challenge
                var code = _twoFactorService.GenerateCode();
                await _twoFactorService.StoreCodeAsync(user.Id, code);

                // Security: Do not log 2FA codes
                var emailBody = $@"Hello {user.UserName},
Your login verification code is:

{code}

This code will expire in 10 minutes.

If you didn't request this login, please ignore this email.

For security, never share this code with anyone.

Best regards,
TalentVerse Team";

                await _emailService.SendEmailAsync(user.Email!, "TalentVerse Login Code", emailBody);

                return ServiceResponse<UserDto>.SuccessResponse(
                    new UserDto
                    {
                        Email = user.Email,
                        IsTwoFactorRequired = true
                    },
                    "Two-factor authentication is required. Please verify the code sent to your email.");
            }

            // Generate token pair instead of single JWT
            var tokenPair = await _tokenService.GenerateTokenPairAsync(user, ipAddress);
            
            // Check if user has password (to determine auth method)
            var hasPassword = await _userManager.HasPasswordAsync(user);

            return ServiceResponse<UserDto>.SuccessResponse(
                new UserDto
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Bio = user.Bio,
                    ProfilePictureUrl = user.ProfilePictureURL,
                    Token = tokenPair.AccessToken, // Hybrid approach: access token in response body
                    IsProfileComplete = user.IsProfileComplete,
                    IsTwoFactorSetupComplete = user.IsTwoFactorSetupComplete,
                    HasPassword = hasPassword // Tells frontend if this is an OAuth user or password user
                    // RefreshToken is set as httpOnly cookie in controller
                },
                AppConstant.SuccessMessages.LoginSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for email {Email}", nameof(LoginAsync), loginDto?.Email);
            return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<CurrentUserDto>> GetCurrentUserAsync(ClaimsPrincipal userPrincipal)
    {
        try
        {
            // Guard clause: null principal
            if (userPrincipal is null)
                return ServiceResponse<CurrentUserDto>.FailureResponse("User context is missing.");

            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                _logger.LogWarning("{Method} could not resolve user from ClaimsPrincipal.", nameof(GetCurrentUserAsync));
                return ServiceResponse<CurrentUserDto>.FailureResponse("User not found.");
            }

            // Check if user has a password (to distinguish OAuth-only users)
            var hasPassword = await _userManager.HasPasswordAsync(user);

            return ServiceResponse<CurrentUserDto>.SuccessResponse(new CurrentUserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureURL,
                IsProfileComplete = user.IsProfileComplete,
                IsTwoFactorSetupComplete = user.IsTwoFactorSetupComplete,
                HasPassword = hasPassword,
                Location = user.Location,
                GitHubUrl = user.GitHubUrl,
                LinkedInUrl = user.LinkedInUrl,
                TwitterUrl = user.TwitterUrl,
                WebsiteUrl = user.WebsiteUrl,
                CreditBalance = user.CreditBalance
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", nameof(GetCurrentUserAsync));
            return ServiceResponse<CurrentUserDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<CurrentUserDto>> UpdateCurrentUserAsync(
    ClaimsPrincipal userPrincipal,
    UpdateProfileDto updateDto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                return ServiceResponse<CurrentUserDto>.FailureResponse("User not found.");

            // Update username if provided and different
            if (!string.IsNullOrWhiteSpace(updateDto.Username))
            {
                var trimmedUsername = updateDto.Username.Trim();
                if (trimmedUsername != user.UserName)
                {
                    // Validate username format/length first
                    if (trimmedUsername.Length < 3 || trimmedUsername.Length > 50)
                        return ServiceResponse<CurrentUserDto>.FailureResponse(
                            "Username must be between 3 and 50 characters.");

                    // Check uniqueness using normalized name
                    var normalizedUsername = _userManager.NormalizeName(trimmedUsername);
                    if (await _userManager.Users.AnyAsync(u =>
                        u.NormalizedUserName == normalizedUsername && u.Id != user.Id))
                    {
                        return ServiceResponse<CurrentUserDto>.FailureResponse(
                            "Username is already taken.");
                    }

                    // Use SetUserNameAsync to properly update both UserName and NormalizedUserName
                    var usernameResult = await _userManager.SetUserNameAsync(user, trimmedUsername);
                    if (!usernameResult.Succeeded)
                    {
                        var errors = string.Join(", ", usernameResult.Errors.Select(e => e.Description));
                        return ServiceResponse<CurrentUserDto>.FailureResponse(
                            $"Username update failed: {errors}");
                    }
                }
            }

            // Update bio if provided
            if (!string.IsNullOrWhiteSpace(updateDto.Bio))
            {
                if (updateDto.Bio.Length > 500)
                    return ServiceResponse<CurrentUserDto>.FailureResponse(
                        "Bio cannot exceed 500 characters.");
                user.Bio = updateDto.Bio.Trim();
            }
            else if (updateDto.Bio == string.Empty)
            {
                user.Bio = null; // Clear bio if empty string sent
            }

            // Update profile picture URL if provided
            if (!string.IsNullOrWhiteSpace(updateDto.ProfilePictureUrl))
            {
                if (!Uri.IsWellFormedUriString(updateDto.ProfilePictureUrl, UriKind.Absolute))
                    return ServiceResponse<CurrentUserDto>.FailureResponse(
                        "Invalid profile picture URL.");
                user.ProfilePictureURL = updateDto.ProfilePictureUrl;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse<CurrentUserDto>.FailureResponse(
                    $"Update failed: {errors}");
            }

            return ServiceResponse<CurrentUserDto>.SuccessResponse(
                new CurrentUserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Bio = user.Bio,
                    ProfilePictureUrl = user.ProfilePictureURL,
                    CreditBalance = user.CreditBalance
                },
                "Profile updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user");
            return ServiceResponse<CurrentUserDto>.FailureResponse(
                AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<string>> RequestTwoFactorCodeAsync(ClaimsPrincipal userPrincipal)
    {
        try
        {
            // Guard clause: null principal
            if (userPrincipal is null)
                return ServiceResponse<string>.FailureResponse("User context is missing.");

            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                _logger.LogWarning("{Method} could not resolve user from ClaimsPrincipal.", nameof(RequestTwoFactorCodeAsync));
                return ServiceResponse<string>.FailureResponse("User not found.");
            }

            // Business rule: already enabled
            if (user.TwoFactorEnabled)
                return ServiceResponse<string>.FailureResponse("Two-factor authentication is already enabled.");

            var code = _twoFactorService.GenerateCode();
            await _twoFactorService.StoreCodeAsync(user.Id, code);

            var emailBody = $@"Hello {user.UserName},

Your two-factor authentication enable code is:

{code}

This code will expire in 10 minutes.

If you didn't request this, please ignore this email.

For security, never share this code with anyone.

Best regards,
TalentVerse Team";
            await _emailService.SendEmailAsync(user.Email, "TalentVerse 2FA Code", emailBody);

            return ServiceResponse<string>.SuccessResponse("", AppConstant.SuccessMessages.OtpSent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", nameof(RequestTwoFactorCodeAsync));
            return ServiceResponse<string>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> EnableTwoFactorAsync(ClaimsPrincipal userPrincipal, VerifyCodeDto dto)
    {
        try
        {
            // Guard clause: null inputs
            if (userPrincipal is null)
                return ServiceResponse<bool>.FailureResponse("User context is missing.");
            if (dto is null)
                return ServiceResponse<bool>.FailureResponse("Verification payload is required.");

            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null) return ServiceResponse<bool>.FailureResponse("User not found.");

            if (user.TwoFactorEnabled)
                return ServiceResponse<bool>.FailureResponse("Two-factor authentication is already enabled.");

            // Input sanitization
            var code = dto.Code?.Trim();

            // Fail-fast: format validation
            if (string.IsNullOrWhiteSpace(code))
                return ServiceResponse<bool>.FailureResponse("Verification code is required.");

            if (code.Length != 6)
                return ServiceResponse<bool>.FailureResponse("Verification code must be exactly 6 characters.");

            var isValid = await _twoFactorService.ValidateCodeAsync(user.Id, code);
            if (!isValid) return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.InvalidOtp);

            var enableResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enableResult.Succeeded)
            {
                var errors = enableResult.Errors.Select(e => e.Description).ToList();
                return ServiceResponse<bool>.FailureResponse("Failed to enable two-factor authentication.", errors);
            }

            // ⭐ Mark 2FA setup as complete
            user.IsTwoFactorSetupComplete = true;
            await _userManager.UpdateAsync(user);

            return ServiceResponse<bool>.SuccessResponse(true, AppConstant.SuccessMessages.TwofaEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", nameof(EnableTwoFactorAsync));
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<UserDto>> LoginWith2faAsync(VerifyTwoFactorDto verifyDto, string ipAddress = "Unknown")
    {
        try
        {
            // Guard clause: null request
            if (verifyDto is null)
                return ServiceResponse<UserDto>.FailureResponse("Verification payload is required.");

            // Input sanitization
            var trimmedEmail = verifyDto.Email?.Trim();
            var trimmedCode = verifyDto.Code?.Trim();

            // Fail-fast: cheap validations
            if (string.IsNullOrWhiteSpace(trimmedEmail))
                return ServiceResponse<UserDto>.FailureResponse("Email is required.");

            if (!Uri.IsWellFormedUriString($"mailto:{trimmedEmail}", UriKind.Absolute))
                return ServiceResponse<UserDto>.FailureResponse("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(trimmedCode))
                return ServiceResponse<UserDto>.FailureResponse("Two-factor code is required.");

            if (trimmedCode.Length != 6)
                return ServiceResponse<UserDto>.FailureResponse("Two-factor code must be exactly 6 characters.");

            // Expensive operation: lookup by normalized email
            var normalizedEmail = _userManager.NormalizeEmail(trimmedEmail);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
            if (user == null)
                return ServiceResponse<UserDto>.FailureResponse("Invalid email or code.");

            if (!user.TwoFactorEnabled)
                return ServiceResponse<UserDto>.FailureResponse("Two-factor authentication is not enabled for this account.");

            // Expensive operation: validate code
            var isValid = await _twoFactorService.ValidateCodeAsync(user.Id, trimmedCode);
            if (!isValid)
                return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.InvalidOtp);

            var tokenPair = await _tokenService.GenerateTokenPairAsync(user, ipAddress);

            return ServiceResponse<UserDto>.SuccessResponse(new UserDto
            {
                Username = user.UserName,
                Email = user.Email,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureURL,
                Token = tokenPair.AccessToken,
                IsProfileComplete = user.IsProfileComplete,
                IsTwoFactorSetupComplete = user.IsTwoFactorSetupComplete,
                HasPassword = await _userManager.HasPasswordAsync(user)
            }, "Login Successful via 2FA");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for email {Email}", nameof(LoginWith2faAsync), verifyDto?.Email);
            return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        try
        {
            // Guard clause: null request
            if (dto is null)
                return ServiceResponse<string>.FailureResponse("Forgot password payload is required.");

            // Input sanitization
            var trimmedEmail = dto.Email?.Trim();

            // Fail-fast: cheap validations
            if (string.IsNullOrWhiteSpace(trimmedEmail))
                return ServiceResponse<string>.FailureResponse("Email is required.");

            if (!Uri.IsWellFormedUriString($"mailto:{trimmedEmail}", UriKind.Absolute))
                return ServiceResponse<string>.FailureResponse("Email format is invalid.");

            // Expensive operation: lookup user
            var normalizedEmail = _userManager.NormalizeEmail(trimmedEmail);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            // Security: Always return success even if user not found to prevent email enumeration
            if (user == null)
                return ServiceResponse<string>.SuccessResponse("", "If the email exists, a reset code has been sent.");

            var code = _twoFactorService.GenerateCode();
            await _twoFactorService.StoreCodeAsync(user.Id, code);

            var emailBody = $@"Hello {user.UserName},

Your password reset verification code is:

{code}

This code will expire in 10 minutes.

If you didn't request this reset, please ignore this email and your password will remain unchanged.

For security, never share this code with anyone.

Best regards,
TalentVerse Team";
            await _emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);

            return ServiceResponse<string>.SuccessResponse("", "If the email exists, a reset code has been sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for email {Email}", nameof(ForgotPasswordAsync), dto?.Email);
            return ServiceResponse<string>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        try
        {
            // Guard clause: null request
            if (dto is null)
                return ServiceResponse<string>.FailureResponse("Reset password payload is required.");

            // Input sanitization
            var trimmedEmail = dto.Email?.Trim();
            var trimmedCode = dto.Code?.Trim();
            var trimmedPassword = dto.NewPassword?.Trim();

            // Fail-fast: cheap validations
            if (string.IsNullOrWhiteSpace(trimmedEmail))
                return ServiceResponse<string>.FailureResponse("Email is required.");

            if (!Uri.IsWellFormedUriString($"mailto:{trimmedEmail}", UriKind.Absolute))
                return ServiceResponse<string>.FailureResponse("Email format is invalid.");

            if (string.IsNullOrWhiteSpace(trimmedCode))
                return ServiceResponse<string>.FailureResponse("Reset code is required.");

            if (trimmedCode.Length != 6)
                return ServiceResponse<string>.FailureResponse("Reset code must be exactly 6 characters.");

            if (string.IsNullOrWhiteSpace(trimmedPassword))
                return ServiceResponse<string>.FailureResponse("New password is required.");

            // Expensive operation: lookup user
            var normalizedEmail = _userManager.NormalizeEmail(trimmedEmail);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
            if (user == null)
                return ServiceResponse<string>.FailureResponse("Invalid email or reset code.");

            // Expensive operation: validate code
            var isValid = await _twoFactorService.ValidateCodeAsync(user.Id, trimmedCode);
            if (!isValid)
                return ServiceResponse<string>.FailureResponse(AppConstant.ErrorMessages.InvalidOtp);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, trimmedPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ServiceResponse<string>.FailureResponse("Password reset failed.", errors);
            }

            return ServiceResponse<string>.SuccessResponse("Password Reset Successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for email {Email}", nameof(ResetPasswordAsync), dto?.Email);
            return ServiceResponse<string>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<UserDto>> CompleteOnboardingAsync(ClaimsPrincipal userPrincipal, CompleteOnboardingDto dto)
    {
        try
        {
            // 1. Guard clause
            if (userPrincipal == null)
                return ServiceResponse<UserDto>.FailureResponse("User authentication required.");

            if (dto == null)
                return ServiceResponse<UserDto>.FailureResponse("Onboarding data is required.");

            // 2. Get user ID from claims
            var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<UserDto>.FailureResponse("User ID not found in authentication token.");

            // 3. Get user from database
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<UserDto>.FailureResponse("User not found.");

            // 4. Input sanitization
            var trimmedLocation = dto.Location?.Trim();
            var trimmedBio = dto.Bio?.Trim();
            var trimmedProfilePictureUrl = dto.ProfilePictureUrl?.Trim();

            // 5. Fail-fast validation
            if (string.IsNullOrWhiteSpace(trimmedLocation))
                return ServiceResponse<UserDto>.FailureResponse("Location is required to complete your profile.");

            if (string.IsNullOrWhiteSpace(trimmedProfilePictureUrl))
                return ServiceResponse<UserDto>.FailureResponse("Profile picture is required to complete your profile.");

            if (!string.IsNullOrEmpty(trimmedBio) && trimmedBio.Length > BioMaxLength)
                return ServiceResponse<UserDto>.FailureResponse($"Bio cannot exceed {BioMaxLength} characters.");

            // 6. Validate social links if provided
            if (dto.SocialLinks != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.SocialLinks.GitHubUrl) && 
                    !Uri.IsWellFormedUriString(dto.SocialLinks.GitHubUrl, UriKind.Absolute))
                    return ServiceResponse<UserDto>.FailureResponse("GitHub URL must be a valid URL.");

                if (!string.IsNullOrWhiteSpace(dto.SocialLinks.LinkedInUrl) && 
                    !Uri.IsWellFormedUriString(dto.SocialLinks.LinkedInUrl, UriKind.Absolute))
                    return ServiceResponse<UserDto>.FailureResponse("LinkedIn URL must be a valid URL.");

                if (!string.IsNullOrWhiteSpace(dto.SocialLinks.TwitterUrl) && 
                    !Uri.IsWellFormedUriString(dto.SocialLinks.TwitterUrl, UriKind.Absolute))
                    return ServiceResponse<UserDto>.FailureResponse("Twitter URL must be a valid URL.");

                if (!string.IsNullOrWhiteSpace(dto.SocialLinks.WebsiteUrl) && 
                    !Uri.IsWellFormedUriString(dto.SocialLinks.WebsiteUrl, UriKind.Absolute))
                    return ServiceResponse<UserDto>.FailureResponse("Website URL must be a valid URL.");
            }

            // 7. Update user profile
            user.Bio = trimmedBio;
            user.ProfilePictureURL = trimmedProfilePictureUrl;
            user.Location = trimmedLocation;
            user.IsProfileComplete = true;

            if (dto.SocialLinks != null)
            {
                user.GitHubUrl = dto.SocialLinks.GitHubUrl?.Trim();
                user.LinkedInUrl = dto.SocialLinks.LinkedInUrl?.Trim();
                user.TwitterUrl = dto.SocialLinks.TwitterUrl?.Trim();
                user.WebsiteUrl = dto.SocialLinks.WebsiteUrl?.Trim();
            }

            user.UpdatedAt = DateTime.UtcNow;

            // 8. Save changes
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Failed to complete onboarding for user {UserId}: {Errors}", 
                    userId, string.Join(", ", errors));
                return ServiceResponse<UserDto>.FailureResponse("Failed to complete profile.", errors);
            }

            // 9. Generate new token with updated claims
            var token = await _tokenService.CreateToken(user);

            // 10. Map to DTO
            var userDto = new UserDto
            {
                Username = user.UserName!,
                Email = user.Email!,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureURL,
                Token = token,
                IsProfileComplete = user.IsProfileComplete,
                Location = user.Location,
                GitHubUrl = user.GitHubUrl,
                LinkedInUrl = user.LinkedInUrl,
                TwitterUrl = user.TwitterUrl,
                WebsiteUrl = user.WebsiteUrl
            };

            _logger.LogInformation("User {UserId} completed onboarding successfully", userId);

            return ServiceResponse<UserDto>.SuccessResponse(
                userDto,
                AppConstant.SuccessMessages.OnboardingCompleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", nameof(CompleteOnboardingAsync));
            return ServiceResponse<UserDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    #region External Authentication

    public async Task<ServiceResponse<ExternalLoginResultDto>> ExternalLoginAsync(ExternalLoginInfo externalLoginInfo, string ipAddress)
    {
        try
        {
            if (externalLoginInfo == null)
                return ServiceResponse<ExternalLoginResultDto>.FailureResponse("External login information is required");

            var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResponse<ExternalLoginResultDto>.FailureResponse("Email not provided by external provider");

            // Check if user already has this external login
            var user = await _userManager.FindByLoginAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey);

            if (user != null)
            {
                if (IsEmailUsername(user))
                {
                    var providerName = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name);
                    var repairedUsername = await GenerateUniqueUsernameAsync(providerName, user.Email ?? email, user.Id);
                    var usernameResult = await _userManager.SetUserNameAsync(user, repairedUsername);
                    if (!usernameResult.Succeeded)
                    {
                        var errors = string.Join(", ", usernameResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Failed to repair OAuth username for user {UserId}: {Errors}", user.Id, errors);
                    }
                }

                await _creditService.AwardSignupBonusAsync(user.Id);
                await _badgeService.EvaluateOnSignupAsync(user.Id);

                // User exists and has this external login - sign them in
                // Generate token pair
                var tokenPair = await _tokenService.GenerateTokenPairAsync(user, ipAddress);

                _logger.LogInformation(
                    "User {UserId} logged in with {Provider}",
                    user.Id,
                    externalLoginInfo.LoginProvider);

                return ServiceResponse<ExternalLoginResultDto>.SuccessResponse(new ExternalLoginResultDto
                {
                    Token = tokenPair.AccessToken, // Hybrid: access token in response
                    IsNewUser = false,
                    RequiresOnboarding = !user.IsProfileComplete,
                    IsTwoFactorSetupComplete = user.IsTwoFactorSetupComplete,
                    Email = user.Email!,
                    ProfilePictureUrl = user.ProfilePictureURL
                }, "Login successful");
            }

            // Check if user exists with this email (account linking scenario)
            user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // Account Conflict: Email exists but external login not linked
                return ServiceResponse<ExternalLoginResultDto>.FailureResponse(
                    $"An account with email {email} already exists. Please log in with your password and link your {externalLoginInfo.LoginProvider} account from profile settings.");
            }

            // New user - create account
            var name = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];
            var picture = externalLoginInfo.Principal.FindFirstValue("picture");
            var generatedUsername = await GenerateUniqueUsernameAsync(name, email);

            var newUser = new AppUser
            {
                UserName = generatedUsername,
                Email = email,
                ProfilePictureURL = picture,
                EmailConfirmed = true, // Trust external provider's email verification
                IsProfileComplete = false, // Require onboarding
                IsTwoFactorSetupComplete = true, // OAuth handles 2FA, skip our setup
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(newUser);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user from external login: {Errors}", errors);
                return ServiceResponse<ExternalLoginResultDto>.FailureResponse($"Failed to create account: {errors}");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(newUser, AppConstant.Roles.Member);
            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to assign role to OAuth user: {Errors}", errors);
                return ServiceResponse<ExternalLoginResultDto>.FailureResponse($"Failed to assign account role: {errors}");
            }

            // Link external login to new user
            var addLoginResult = await _userManager.AddLoginAsync(newUser, externalLoginInfo);

            if (!addLoginResult.Succeeded)
            {
                // Rollback user creation
                await _userManager.DeleteAsync(newUser);
                var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to link external login: {Errors}", errors);
                return ServiceResponse<ExternalLoginResultDto>.FailureResponse($"Failed to link account: {errors}");
            }

            await _creditService.AwardSignupBonusAsync(newUser.Id);
            await _badgeService.EvaluateOnSignupAsync(newUser.Id);

            // Generate token pair for new user
            // ⭐ Generate token pair
            var newUserTokenPair = await _tokenService.GenerateTokenPairAsync(newUser, ipAddress);

            _logger.LogInformation(
                "New user {UserId} created via {Provider}",
                newUser.Id,
                externalLoginInfo.LoginProvider);

            return ServiceResponse<ExternalLoginResultDto>.SuccessResponse(new ExternalLoginResultDto
            {
                Token = newUserTokenPair.AccessToken, // Hybrid: access token in response
                IsNewUser = true,
                RequiresOnboarding = true,
                IsTwoFactorSetupComplete = true, // OAuth users skip 2FA setup
                Email = newUser.Email!,
                ProfilePictureUrl = newUser.ProfilePictureURL
            }, "Account created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during external login");
            return ServiceResponse<ExternalLoginResultDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<IEnumerable<LinkedLoginDto>>> GetExternalLoginsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<IEnumerable<LinkedLoginDto>>.FailureResponse("User ID is required");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResponse<IEnumerable<LinkedLoginDto>>.FailureResponse("User not found");

            var logins = await _userManager.GetLoginsAsync(user);

            var linkedLogins = logins.Select(l => new LinkedLoginDto
            {
                Provider = l.LoginProvider,
                ProviderDisplayName = l.ProviderDisplayName ?? l.LoginProvider,
                ProviderKey = l.ProviderKey
            }).ToList();

            return ServiceResponse<IEnumerable<LinkedLoginDto>>.SuccessResponse(linkedLogins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving external logins for user {UserId}", userId);
            return ServiceResponse<IEnumerable<LinkedLoginDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> LinkExternalLoginAsync(string userId, ExternalLoginInfo externalLoginInfo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<bool>.FailureResponse("User ID is required");

            if (externalLoginInfo == null)
                return ServiceResponse<bool>.FailureResponse("External login information is required");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResponse<bool>.FailureResponse("User not found");

            // Check if this external login is already linked to another user
            var existingUser = await _userManager.FindByLoginAsync(
                externalLoginInfo.LoginProvider,
                externalLoginInfo.ProviderKey);

            if (existingUser != null && existingUser.Id != userId)
            {
                // Conflict: This external account is already linked to another user
                return ServiceResponse<bool>.FailureResponse(
                    $"This {externalLoginInfo.LoginProvider} account is already linked to another user.");
            }

            // Link the external login
            var result = await _userManager.AddLoginAsync(user, externalLoginInfo);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse<bool>.FailureResponse($"Failed to link account: {errors}");
            }

            _logger.LogInformation(
                "User {UserId} linked {Provider} account",
                userId,
                externalLoginInfo.LoginProvider);

            return ServiceResponse<bool>.SuccessResponse(
                true,
                $"{externalLoginInfo.LoginProvider} account linked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking external login for user {UserId}", userId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> UnlinkExternalLoginAsync(string userId, string loginProvider)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<bool>.FailureResponse("User ID is required");

            if (string.IsNullOrWhiteSpace(loginProvider))
                return ServiceResponse<bool>.FailureResponse("Login provider is required");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResponse<bool>.FailureResponse("User not found");

            // Get all external logins
            var logins = await _userManager.GetLoginsAsync(user);

            // Check if user has a password (for account recovery)
            var hasPassword = await _userManager.HasPasswordAsync(user);

            // Don't allow unlinking if it's the only login method and user has no password
            if (logins.Count == 1 && !hasPassword)
            {
                return ServiceResponse<bool>.FailureResponse(
                    "Cannot unlink the only login method. Please set a password first.");
            }

            // Find the login to remove
            var loginToRemove = logins.FirstOrDefault(l => l.LoginProvider == loginProvider);

            if (loginToRemove == null)
                return ServiceResponse<bool>.FailureResponse($"{loginProvider} account is not linked");

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, loginToRemove.ProviderKey);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse<bool>.FailureResponse($"Failed to unlink account: {errors}");
            }

            _logger.LogInformation(
                "User {UserId} unlinked {Provider} account",
                userId,
                loginProvider);

            return ServiceResponse<bool>.SuccessResponse(
                true,
                $"{loginProvider} account unlinked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking external login for user {UserId}", userId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    #endregion
}
