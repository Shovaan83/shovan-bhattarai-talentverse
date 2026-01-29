using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Account;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService, 
        ICloudinaryService cloudinaryService,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="registerDto">User registration details</param>
    /// <returns>Newly created user with authentication token</returns>
    /// <response code="200">User successfully registered</response>
    /// <response code="400">Validation failed or username/email already exists</response>
    [HttpPost("register")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<UserDto>>> Register([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse(
                "Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.RegisterAsync(registerDto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="loginDto">User login credentials</param>
    /// <returns>User details with JWT token or 2FA challenge</returns>
    /// <response code="200">Login successful or 2FA required</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="403">Email not confirmed</response>
    /// <response code="423">Account locked</response>
    [HttpPost("login")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), 423)]
    public async Task<ActionResult<ServiceResponse<UserDto>>> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse("Validation failed"));

        var result = await _authService.LoginAsync(loginDto);

        if (!result.Success)
        {
            // Return appropriate status code based on error type
            if (result.Message?.Contains("Invalid", StringComparison.OrdinalIgnoreCase) == true ||
                result.Message?.Contains("password", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Unauthorized(result);
            }

            if (result.Message?.Contains("locked", StringComparison.OrdinalIgnoreCase) == true)
            {
                return StatusCode(423, result); // 423 Locked
            }

            if (result.Message?.Contains("confirm", StringComparison.OrdinalIgnoreCase) == true)
            {
                return StatusCode(403, result); // 403 Forbidden
            }

            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets the current authenticated user's profile
    /// </summary>
    /// <returns>Current user profile details</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">User not found in database</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ServiceResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ServiceResponse<CurrentUserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse<CurrentUserDto>>> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUserAsync(User);

        if (!result.Success)
        {
            // User authenticated but not found in DB - data integrity issue
            _logger.LogWarning("Authenticated user not found in database");
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Updates the current authenticated user's profile
    /// </summary>
    /// <param name="updateDto">Profile update details</param>
    /// <returns>Updated user profile</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Validation failed or username already taken</response>
    /// <response code="401">User not authenticated</response>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(ServiceResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<CurrentUserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<CurrentUserDto>>> UpdateCurrentUser([FromBody] UpdateProfileDto updateDto)
    {
        if (updateDto == null)
            return BadRequest(ServiceResponse<CurrentUserDto>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<CurrentUserDto>.FailureResponse("Validation failed"));

        var result = await _authService.UpdateCurrentUserAsync(User, updateDto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Requests a 2FA verification code to enable two-factor authentication
    /// </summary>
    /// <returns>Success message indicating code was sent</returns>
    /// <response code="200">Verification code sent successfully</response>
    /// <response code="400">2FA already enabled or request failed</response>
    /// <response code="401">User not authenticated</response>
    [Authorize]
    [HttpPost("request-2fa-code")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<string>>> RequestTwoFactorCode()
    {
        var result = await _authService.RequestTwoFactorCodeAsync(User);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Enables two-factor authentication for the current user
    /// </summary>
    /// <param name="verifyDto">2FA verification code</param>
    /// <returns>Success status</returns>
    /// <response code="200">2FA enabled successfully</response>
    /// <response code="400">Invalid code or 2FA already enabled</response>
    /// <response code="401">User not authenticated</response>
    [Authorize]
    [HttpPost("enable-2fa")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<bool>>> EnableTwoFactor([FromBody] VerifyCodeDto verifyDto)
    {
        if (verifyDto == null)
            return BadRequest(ServiceResponse<bool>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<bool>.FailureResponse("Validation failed"));

        var result = await _authService.EnableTwoFactorAsync(User, verifyDto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Completes login with two-factor authentication code
    /// </summary>
    /// <param name="verifyDto">Email and 2FA code</param>
    /// <returns>User details with JWT token</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">Invalid code or 2FA not enabled</response>
    [HttpPost("login-2fa")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<UserDto>>> LoginWith2FA([FromBody] VerifyTwoFactorDto verifyDto)
    {
        if (verifyDto == null)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse("Validation failed"));

        var result = await _authService.LoginWith2faAsync(verifyDto);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Initiates password reset process by sending a reset code to the user's email
    /// </summary>
    /// <param name="dto">User's email address</param>
    /// <returns>Success message (always returns 200 for security)</returns>
    /// <response code="200">Request processed (code sent if email exists)</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (dto == null)
            return BadRequest(ServiceResponse<string>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<string>.FailureResponse("Validation failed"));

        var result = await _authService.ForgotPasswordAsync(dto);

        // Always return 200 OK to prevent email enumeration
        // Service handles security internally
        return Ok(result);
    }

    /// <summary>
    /// Resets user password using the verification code sent to their email
    /// </summary>
    /// <param name="dto">Email, reset code, and new password</param>
    /// <returns>Success or failure status</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Validation failed or invalid reset code</response>
    [HttpPost("reset-password")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<string>>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (dto == null)
            return BadRequest(ServiceResponse<string>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<string>.FailureResponse("Validation failed"));

        var result = await _authService.ResetPasswordAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Uploads profile picture to Cloudinary with validation (max 5MB, JPEG/PNG/WebP only)
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <returns>Image URL and metadata</returns>
    /// <response code="200">Image uploaded successfully</response>
    /// <response code="400">Validation failed or invalid image format</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("upload-profile-picture")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<ImageUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<ImageUploadResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<ImageUploadResultDto>>> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ServiceResponse<ImageUploadResultDto>.FailureResponse(
                AppConstant.ErrorMessages.NoImageProvided));

        var result = await _cloudinaryService.UploadImageAsync(file);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Completes user onboarding by setting profile picture, bio, location, and social links
    /// </summary>
    /// <param name="dto">Onboarding data including profile picture URL, bio, location, and social links</param>
    /// <returns>Success message</returns>
    /// <response code="200">Profile completed successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("complete-onboarding")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<UserDto>>> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
    {
        if (dto == null)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse("Request body is required"));

        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<UserDto>.FailureResponse(
                "Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _authService.CompleteOnboardingAsync(User, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }}
