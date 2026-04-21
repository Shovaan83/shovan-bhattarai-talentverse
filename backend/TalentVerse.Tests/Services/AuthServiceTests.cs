using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Account;
using TalentVerse.WebAPI.Interfaces;
using TalentVerse.WebAPI.Services;

namespace TalentVerse.Tests.Services
{
    public class AuthServiceTests
    {
        #region Test Setup

        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ITwoFactorService> _twoFactorServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ICreditService> _creditServiceMock;
        private readonly Mock<IBadgeService> _badgeServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _sut; // System Under Test

        // Common test data
        private const string ValidEmail = "test@example.com";
        private const string ValidUsername = "testuser";
        private const string ValidPassword = "SecurePass123!";
        private const string ValidUserId = "user-id-123";
        private const string ValidToken = "jwt-token-abc123";
        private const string ValidRefreshToken = "refresh-token-xyz";
        private const string ValidIpAddress = "127.0.0.1";
        private const string Valid2faCode = "123456";

        public AuthServiceTests()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Setup SignInManager mock
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            _signInManagerMock = new Mock<SignInManager<AppUser>>(
                _userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

            _tokenServiceMock = new Mock<ITokenService>();
            _twoFactorServiceMock = new Mock<ITwoFactorService>();
            _emailServiceMock = new Mock<IEmailService>();
            _creditServiceMock = new Mock<ICreditService>();
            _badgeServiceMock = new Mock<IBadgeService>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _sut = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _twoFactorServiceMock.Object,
                _emailServiceMock.Object,
                _creditServiceMock.Object,
                _badgeServiceMock.Object,
                _loggerMock.Object);
        }

        private static AppUser CreateTestUser(
            string? id = ValidUserId,
            string? email = ValidEmail,
            string? username = ValidUsername,
            bool isProfileComplete = false,
            bool twoFactorEnabled = false,
            bool isTwoFactorSetupComplete = false)
        {
            return new AppUser
            {
                Id = id ?? ValidUserId,
                Email = email ?? ValidEmail,
                UserName = username ?? ValidUsername,
                NormalizedEmail = (email ?? ValidEmail).ToUpperInvariant(),
                NormalizedUserName = (username ?? ValidUsername).ToUpperInvariant(),
                IsProfileComplete = isProfileComplete,
                TwoFactorEnabled = twoFactorEnabled,
                IsTwoFactorSetupComplete = isTwoFactorSetupComplete,
                Bio = "Test bio",
                CreditBalance = 100m
            };
        }

        private static ClaimsPrincipal CreateTestClaimsPrincipal(string userId = ValidUserId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, ValidEmail),
                new(ClaimTypes.Name, ValidUsername)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private void SetupUserManagerNormalization()
        {
            _userManagerMock
                .Setup(x => x.NormalizeEmail(It.IsAny<string>()))
                .Returns<string>(email => email?.ToUpperInvariant() ?? "");

            _userManagerMock
                .Setup(x => x.NormalizeName(It.IsAny<string>()))
                .Returns<string>(name => name?.ToUpperInvariant() ?? "");
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_ValidInput_ReturnsSuccessWithUserDto()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ValidPassword,
                Bio = "Hello World"
            };

            SetupUserManagerNormalization();

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), ValidPassword))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), AppConstant.Roles.Member))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.CreateToken(It.IsAny<AppUser>()))
                .ReturnsAsync(ValidToken);

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Username.Should().Be(ValidUsername);
            result.Data.Email.Should().Be(ValidEmail);
            result.Data.Token.Should().Be(ValidToken);
            result.Message.Should().Be(AppConstant.SuccessMessages.RegistrationSuccessful);
        }

        [Fact]
        public async Task RegisterAsync_AwardsSignupBonusAndBadge()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ValidPassword
            };

            SetupUserManagerNormalization();

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), ValidPassword))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), AppConstant.Roles.Member))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.CreateToken(It.IsAny<AppUser>()))
                .ReturnsAsync(ValidToken);

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            _creditServiceMock.Verify(x => x.AwardSignupBonusAsync(It.IsAny<string>()), Times.Once);
            _badgeServiceMock.Verify(x => x.EvaluateOnSignupAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_NullDto_ReturnsFailure()
        {
            // Act
            var result = await _sut.RegisterAsync(null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Registration payload is required");
        }

        [Fact]
        public async Task RegisterAsync_EmptyUsername_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = "",
                Password = ValidPassword
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Username is required");
        }

        [Fact]
        public async Task RegisterAsync_UsernameTooShort_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = "ab", // Less than 3 characters
                Password = ValidPassword
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("between 3 and 50 characters");
        }

        [Fact]
        public async Task RegisterAsync_UsernameTooLong_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = new string('a', 51), // More than 50 characters
                Password = ValidPassword
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("between 3 and 50 characters");
        }

        [Fact]
        public async Task RegisterAsync_UsernameInvalidCharacters_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = "test@user!", // Invalid characters
                Password = ValidPassword
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("only contain letters, numbers, and underscores");
        }

        [Fact]
        public async Task RegisterAsync_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "",
                Username = ValidUsername,
                Password = ValidPassword
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email is required");
        }

        [Fact]
        public async Task RegisterAsync_InvalidEmailFormat_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "not-a-valid-email",
                Username = ValidUsername,
                Password = ValidPassword
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email format is invalid");
        }

        [Fact]
        public async Task RegisterAsync_EmptyPassword_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ""
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Password is required");
        }

        [Fact]
        public async Task RegisterAsync_BioTooLong_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ValidPassword,
                Bio = new string('a', 501) // More than 500 characters
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Bio cannot exceed 500 characters");
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ValidPassword
            };

            var existingUsers = new List<AppUser> { CreateTestUser() };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(existingUsers.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserExists);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "newemail@example.com",
                Username = ValidUsername,
                Password = ValidPassword
            };

            var existingUsers = new List<AppUser> { CreateTestUser() };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(existingUsers.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Username is already taken");
        }

        [Fact]
        public async Task RegisterAsync_CreateAsyncFails_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = "weak"
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User creation failed");
            result.Errors.Should().Contain("Password too weak");
        }

        [Fact]
        public async Task RegisterAsync_AddToRoleFails_ReturnsFailure()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ValidPassword
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), ValidPassword))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), AppConstant.Roles.Member))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role not found" }));

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("role assignment failed");
        }

        [Fact]
        public async Task RegisterAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = ValidUsername,
                Password = ValidPassword
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Throws(new Exception("Database connection failed"));

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithUserDto()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = ValidPassword };
            var user = CreateTestUser();

            SetupUserManagerNormalization();

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, ValidPassword))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.ResetAccessFailedCountAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);

            _userManagerMock.Object.Options.SignIn.RequireConfirmedEmail = false;

            _tokenServiceMock
                .Setup(x => x.GenerateTokenPairAsync(user, ValidIpAddress))
                .ReturnsAsync((ValidToken, ValidRefreshToken, DateTime.UtcNow.AddDays(7)));

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Token.Should().Be(ValidToken);
            result.Message.Should().Be(AppConstant.SuccessMessages.LoginSuccessful);
        }

        [Fact]
        public async Task LoginAsync_NullDto_ReturnsFailure()
        {
            // Act
            var result = await _sut.LoginAsync(null!, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Login payload is required");
        }

        [Fact]
        public async Task LoginAsync_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "", Password = ValidPassword };

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email is required");
        }

        [Fact]
        public async Task LoginAsync_InvalidEmailFormat_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "not-an-email", Password = ValidPassword };

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email format is invalid");
        }

        [Fact]
        public async Task LoginAsync_EmptyPassword_ReturnsFailure()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = "" };

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Password is required");
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsInvalidLogin()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = ValidPassword };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidLogin);
        }

        [Fact]
        public async Task LoginAsync_UserLockedOut_ReturnsLockoutMessage()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = ValidPassword };
            var user = CreateTestUser();

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetLockoutEndDateAsync(user))
                .ReturnsAsync(DateTimeOffset.UtcNow.AddMinutes(15));

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Account is locked");
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsInvalidLogin()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = "wrongpassword" };
            var user = CreateTestUser();

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.AccessFailedAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidLogin);
        }

        [Fact]
        public async Task LoginAsync_InvalidPasswordTriggersLockout_ReturnsLockoutMessage()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = "wrongpassword" };
            var user = CreateTestUser();

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .SetupSequence(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false)
                .ReturnsAsync(true); // Locked out after failed attempt

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, "wrongpassword"))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.AccessFailedAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("locked");
        }

        [Fact]
        public async Task LoginAsync_TwoFactorEnabled_SendsCodeAndReturns2faRequired()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = ValidPassword };
            var user = CreateTestUser(twoFactorEnabled: true);

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, ValidPassword))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.ResetAccessFailedCountAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Object.Options.SignIn.RequireConfirmedEmail = false;

            _twoFactorServiceMock
                .Setup(x => x.GenerateCode())
                .Returns(Valid2faCode);

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.IsTwoFactorRequired.Should().BeTrue();
            result.Message.Should().Contain("Two-factor authentication is required");

            _twoFactorServiceMock.Verify(x => x.StoreCodeAsync(user.Id, Valid2faCode), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                user.Email!, 
                It.Is<string>(s => s.Contains("Login Code")), 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var loginDto = new LoginDto { Email = ValidEmail, Password = ValidPassword };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Throws(new Exception("Database error"));

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region GetCurrentUserAsync Tests

        [Fact]
        public async Task GetCurrentUserAsync_ValidPrincipal_ReturnsCurrentUserDto()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.GetCurrentUserAsync(principal);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.Username.Should().Be(user.UserName);
            result.Data.Email.Should().Be(user.Email);
            result.Data.HasPassword.Should().BeTrue();
        }

        [Fact]
        public async Task GetCurrentUserAsync_NullPrincipal_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetCurrentUserAsync(null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User context is missing");
        }

        [Fact]
        public async Task GetCurrentUserAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.GetCurrentUserAsync(principal);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User not found");
        }

        [Fact]
        public async Task GetCurrentUserAsync_OAuthUserWithNoPassword_ReturnsHasPasswordFalse()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.GetCurrentUserAsync(principal);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.HasPassword.Should().BeFalse();
        }

        [Fact]
        public async Task GetCurrentUserAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetCurrentUserAsync(principal);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region UpdateCurrentUserAsync Tests

        [Fact]
        public async Task UpdateCurrentUserAsync_ValidInput_ReturnsUpdatedUser()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto
            {
                Bio = "Updated bio"
            };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Bio.Should().Be("Updated bio");
            result.Message.Should().Contain("updated successfully");
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_UpdateUsername_ValidatesAndUpdates()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { Username = "newusername" };

            SetupUserManagerNormalization();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.SetUserNameAsync(user, "newusername"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_UsernameTooShort_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { Username = "ab" };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("between 3 and 50 characters");
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_UsernameAlreadyTaken_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var existingUser = CreateTestUser(id: "other-id", username: "newusername");
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { Username = "newusername" };

            SetupUserManagerNormalization();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { existingUser }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already taken");
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_BioTooLong_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { Bio = new string('a', 501) };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("cannot exceed 500 characters");
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_InvalidProfilePictureUrl_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { ProfilePictureUrl = "not-a-valid-url" };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid profile picture URL");
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { Bio = "New bio" };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User not found");
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_ClearBioWithEmptyString_SetsBioToNull()
        {
            // Arrange
            var user = CreateTestUser();
            user.Bio = "Existing bio";
            var principal = CreateTestClaimsPrincipal();
            var updateDto = new UpdateProfileDto { Bio = "" }; // Empty string to clear

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.UpdateCurrentUserAsync(principal, updateDto);

            // Assert
            result.Success.Should().BeTrue();
            user.Bio.Should().BeNull();
        }

        #endregion

        #region RequestTwoFactorCodeAsync Tests

        [Fact]
        public async Task RequestTwoFactorCodeAsync_ValidUser_SendsCodeAndReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: false);
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _twoFactorServiceMock
                .Setup(x => x.GenerateCode())
                .Returns(Valid2faCode);

            // Act
            var result = await _sut.RequestTwoFactorCodeAsync(principal);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be(AppConstant.SuccessMessages.OtpSent);

            _twoFactorServiceMock.Verify(x => x.StoreCodeAsync(user.Id, Valid2faCode), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                user.Email!, 
                It.Is<string>(s => s.Contains("2FA Code")), 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RequestTwoFactorCodeAsync_NullPrincipal_ReturnsFailure()
        {
            // Act
            var result = await _sut.RequestTwoFactorCodeAsync(null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User context is missing");
        }

        [Fact]
        public async Task RequestTwoFactorCodeAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.RequestTwoFactorCodeAsync(principal);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User not found");
        }

        [Fact]
        public async Task RequestTwoFactorCodeAsync_TwoFactorAlreadyEnabled_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: true);
            var principal = CreateTestClaimsPrincipal();

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.RequestTwoFactorCodeAsync(principal);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already enabled");
        }

        #endregion

        #region EnableTwoFactorAsync Tests

        [Fact]
        public async Task EnableTwoFactorAsync_ValidCode_EnablesTwoFactorAndReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: false);
            var principal = CreateTestClaimsPrincipal();
            var verifyDto = new VerifyCodeDto { Code = Valid2faCode };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, Valid2faCode))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.SetTwoFactorEnabledAsync(user, true))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.EnableTwoFactorAsync(principal, verifyDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be(AppConstant.SuccessMessages.TwofaEnabled);
            user.IsTwoFactorSetupComplete.Should().BeTrue();
        }

        [Fact]
        public async Task EnableTwoFactorAsync_NullPrincipal_ReturnsFailure()
        {
            // Act
            var result = await _sut.EnableTwoFactorAsync(null!, new VerifyCodeDto { Code = Valid2faCode });

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User context is missing");
        }

        [Fact]
        public async Task EnableTwoFactorAsync_NullDto_ReturnsFailure()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();

            // Act
            var result = await _sut.EnableTwoFactorAsync(principal, null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Verification payload is required");
        }

        [Fact]
        public async Task EnableTwoFactorAsync_EmptyCode_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var verifyDto = new VerifyCodeDto { Code = "" };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.EnableTwoFactorAsync(principal, verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Verification code is required");
        }

        [Fact]
        public async Task EnableTwoFactorAsync_CodeWrongLength_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var verifyDto = new VerifyCodeDto { Code = "12345" }; // Only 5 chars

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.EnableTwoFactorAsync(principal, verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("exactly 6 characters");
        }

        [Fact]
        public async Task EnableTwoFactorAsync_InvalidCode_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: false);
            var principal = CreateTestClaimsPrincipal();
            var verifyDto = new VerifyCodeDto { Code = "000000" };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, "000000"))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.EnableTwoFactorAsync(principal, verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidOtp);
        }

        [Fact]
        public async Task EnableTwoFactorAsync_AlreadyEnabled_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: true);
            var principal = CreateTestClaimsPrincipal();
            var verifyDto = new VerifyCodeDto { Code = Valid2faCode };

            _userManagerMock
                .Setup(x => x.GetUserAsync(principal))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.EnableTwoFactorAsync(principal, verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already enabled");
        }

        #endregion

        #region LoginWith2faAsync Tests

        [Fact]
        public async Task LoginWith2faAsync_ValidCode_ReturnsSuccessWithToken()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: true);
            var verifyDto = new VerifyTwoFactorDto { Email = ValidEmail, Code = Valid2faCode };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, Valid2faCode))
                .ReturnsAsync(true);

            _tokenServiceMock
                .Setup(x => x.CreateToken(user))
                .ReturnsAsync(ValidToken);

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Token.Should().Be(ValidToken);
            result.Message.Should().Contain("Login Successful via 2FA");
        }

        [Fact]
        public async Task LoginWith2faAsync_NullDto_ReturnsFailure()
        {
            // Act
            var result = await _sut.LoginWith2faAsync(null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Verification payload is required");
        }

        [Fact]
        public async Task LoginWith2faAsync_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var verifyDto = new VerifyTwoFactorDto { Email = "", Code = Valid2faCode };

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email is required");
        }

        [Fact]
        public async Task LoginWith2faAsync_EmptyCode_ReturnsFailure()
        {
            // Arrange
            var verifyDto = new VerifyTwoFactorDto { Email = ValidEmail, Code = "" };

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Two-factor code is required");
        }

        [Fact]
        public async Task LoginWith2faAsync_CodeWrongLength_ReturnsFailure()
        {
            // Arrange
            var verifyDto = new VerifyTwoFactorDto { Email = ValidEmail, Code = "12345" };

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("exactly 6 characters");
        }

        [Fact]
        public async Task LoginWith2faAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var verifyDto = new VerifyTwoFactorDto { Email = ValidEmail, Code = Valid2faCode };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid email or code");
        }

        [Fact]
        public async Task LoginWith2faAsync_TwoFactorNotEnabled_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: false);
            var verifyDto = new VerifyTwoFactorDto { Email = ValidEmail, Code = Valid2faCode };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("not enabled for this account");
        }

        [Fact]
        public async Task LoginWith2faAsync_InvalidCode_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser(twoFactorEnabled: true);
            var verifyDto = new VerifyTwoFactorDto { Email = ValidEmail, Code = "000000" };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, "000000"))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.LoginWith2faAsync(verifyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidOtp);
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Fact]
        public async Task ForgotPasswordAsync_ValidEmail_SendsCodeAndReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            var forgotDto = new ForgotPasswordDto { Email = ValidEmail };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _twoFactorServiceMock
                .Setup(x => x.GenerateCode())
                .Returns(Valid2faCode);

            // Act
            var result = await _sut.ForgotPasswordAsync(forgotDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("If the email exists");

            _twoFactorServiceMock.Verify(x => x.StoreCodeAsync(user.Id, Valid2faCode), Times.Once);
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                user.Email!, 
                It.Is<string>(s => s.Contains("Reset Password")), 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_UserNotFound_StillReturnsSuccessForSecurity()
        {
            // Arrange
            var forgotDto = new ForgotPasswordDto { Email = "nonexistent@example.com" };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.ForgotPasswordAsync(forgotDto);

            // Assert - Returns success to prevent email enumeration
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("If the email exists");

            // No email should be sent
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_NullDto_ReturnsFailure()
        {
            // Act
            var result = await _sut.ForgotPasswordAsync(null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Forgot password payload is required");
        }

        [Fact]
        public async Task ForgotPasswordAsync_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var forgotDto = new ForgotPasswordDto { Email = "" };

            // Act
            var result = await _sut.ForgotPasswordAsync(forgotDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email is required");
        }

        #endregion

        #region ResetPasswordAsync Tests

        [Fact]
        public async Task ResetPasswordAsync_ValidInput_ResetsPasswordAndReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            var resetDto = new ResetPasswordDto
            {
                Email = ValidEmail,
                Code = Valid2faCode,
                NewPassword = "NewSecurePass123!"
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, Valid2faCode))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, "reset-token", "NewSecurePass123!"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().Contain("Password Reset Successful");
        }

        [Fact]
        public async Task ResetPasswordAsync_NullDto_ReturnsFailure()
        {
            // Act
            var result = await _sut.ResetPasswordAsync(null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Reset password payload is required");
        }

        [Fact]
        public async Task ResetPasswordAsync_EmptyEmail_ReturnsFailure()
        {
            // Arrange
            var resetDto = new ResetPasswordDto { Email = "", Code = Valid2faCode, NewPassword = "NewPass123!" };

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Email is required");
        }

        [Fact]
        public async Task ResetPasswordAsync_EmptyCode_ReturnsFailure()
        {
            // Arrange
            var resetDto = new ResetPasswordDto { Email = ValidEmail, Code = "", NewPassword = "NewPass123!" };

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Reset code is required");
        }

        [Fact]
        public async Task ResetPasswordAsync_CodeWrongLength_ReturnsFailure()
        {
            // Arrange
            var resetDto = new ResetPasswordDto { Email = ValidEmail, Code = "12345", NewPassword = "NewPass123!" };

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("exactly 6 characters");
        }

        [Fact]
        public async Task ResetPasswordAsync_EmptyNewPassword_ReturnsFailure()
        {
            // Arrange
            var resetDto = new ResetPasswordDto { Email = ValidEmail, Code = Valid2faCode, NewPassword = "" };

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("New password is required");
        }

        [Fact]
        public async Task ResetPasswordAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var resetDto = new ResetPasswordDto
            {
                Email = "nonexistent@example.com",
                Code = Valid2faCode,
                NewPassword = "NewPass123!"
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid email or reset code");
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidCode_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var resetDto = new ResetPasswordDto
            {
                Email = ValidEmail,
                Code = "000000",
                NewPassword = "NewPass123!"
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, "000000"))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidOtp);
        }

        [Fact]
        public async Task ResetPasswordAsync_WeakPassword_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var resetDto = new ResetPasswordDto
            {
                Email = ValidEmail,
                Code = Valid2faCode,
                NewPassword = "weak"
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _twoFactorServiceMock
                .Setup(x => x.ValidateCodeAsync(user.Id, Valid2faCode))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, "reset-token", "weak"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            // Act
            var result = await _sut.ResetPasswordAsync(resetDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Password reset failed");
            result.Errors.Should().Contain("Password too weak");
        }

        #endregion

        #region CompleteOnboardingAsync Tests

        [Fact]
        public async Task CompleteOnboardingAsync_ValidInput_CompletesProfileAndReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser(isProfileComplete: false);
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "New York, USA",
                ProfilePictureUrl = "https://example.com/photo.jpg",
                Bio = "I love coding"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.CreateToken(user))
                .ReturnsAsync(ValidToken);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.IsProfileComplete.Should().BeTrue();
            result.Data.Location.Should().Be("New York, USA");
            result.Message.Should().Be(AppConstant.SuccessMessages.OnboardingCompleted);
            user.IsProfileComplete.Should().BeTrue();
        }

        [Fact]
        public async Task CompleteOnboardingAsync_WithSocialLinks_UpdatesSocialLinks()
        {
            // Arrange
            var user = CreateTestUser(isProfileComplete: false);
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "London, UK",
                ProfilePictureUrl = "https://example.com/photo.jpg",
                SocialLinks = new SocialLinksDto
                {
                    GitHubUrl = "https://github.com/testuser",
                    LinkedInUrl = "https://linkedin.com/in/testuser"
                }
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.CreateToken(user))
                .ReturnsAsync(ValidToken);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.GitHubUrl.Should().Be("https://github.com/testuser");
            result.Data.LinkedInUrl.Should().Be("https://linkedin.com/in/testuser");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_NullPrincipal_ReturnsFailure()
        {
            // Act
            var result = await _sut.CompleteOnboardingAsync(null!, new CompleteOnboardingDto());

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User authentication required");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_NullDto_ReturnsFailure()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Onboarding data is required");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_MissingLocation_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "",
                ProfilePictureUrl = "https://example.com/photo.jpg"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Location is required");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_MissingProfilePicture_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "New York",
                ProfilePictureUrl = ""
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Profile picture is required");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_BioTooLong_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "New York",
                ProfilePictureUrl = "https://example.com/photo.jpg",
                Bio = new string('a', 501)
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Bio cannot exceed 500 characters");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_InvalidGitHubUrl_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "New York",
                ProfilePictureUrl = "https://example.com/photo.jpg",
                SocialLinks = new SocialLinksDto { GitHubUrl = "not-a-valid-url" }
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("GitHub URL must be a valid URL");
        }

        [Fact]
        public async Task CompleteOnboardingAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var principal = CreateTestClaimsPrincipal();
            var onboardingDto = new CompleteOnboardingDto
            {
                Location = "New York",
                ProfilePictureUrl = "https://example.com/photo.jpg"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.CompleteOnboardingAsync(principal, onboardingDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User not found");
        }

        #endregion

        #region GetExternalLoginsAsync Tests

        [Fact]
        public async Task GetExternalLoginsAsync_ValidUser_ReturnsLinkedLogins()
        {
            // Arrange
            var user = CreateTestUser();
            var logins = new List<UserLoginInfo>
            {
                new("Google", "google-key", "Google"),
                new("GitHub", "github-key", "GitHub")
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(logins);

            // Act
            var result = await _sut.GetExternalLoginsAsync(ValidUserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data!.Should().Contain(l => l.Provider == "Google");
            result.Data!.Should().Contain(l => l.Provider == "GitHub");
        }

        [Fact]
        public async Task GetExternalLoginsAsync_EmptyUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetExternalLoginsAsync("");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task GetExternalLoginsAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.GetExternalLoginsAsync(ValidUserId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User not found");
        }

        #endregion

        #region UnlinkExternalLoginAsync Tests

        [Fact]
        public async Task UnlinkExternalLoginAsync_ValidInput_UnlinksAndReturnsSuccess()
        {
            // Arrange
            var user = CreateTestUser();
            var logins = new List<UserLoginInfo>
            {
                new("Google", "google-key", "Google"),
                new("GitHub", "github-key", "GitHub")
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(logins);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.RemoveLoginAsync(user, "Google", "google-key"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _sut.UnlinkExternalLoginAsync(ValidUserId, "Google");

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("unlinked successfully");
        }

        [Fact]
        public async Task UnlinkExternalLoginAsync_EmptyUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.UnlinkExternalLoginAsync("", "Google");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task UnlinkExternalLoginAsync_EmptyProvider_ReturnsFailure()
        {
            // Act
            var result = await _sut.UnlinkExternalLoginAsync(ValidUserId, "");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Login provider is required");
        }

        [Fact]
        public async Task UnlinkExternalLoginAsync_OnlyLoginMethodWithNoPassword_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var logins = new List<UserLoginInfo>
            {
                new("Google", "google-key", "Google")
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(logins);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(false); // OAuth-only user

            // Act
            var result = await _sut.UnlinkExternalLoginAsync(ValidUserId, "Google");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Cannot unlink the only login method");
        }

        [Fact]
        public async Task UnlinkExternalLoginAsync_ProviderNotLinked_ReturnsFailure()
        {
            // Arrange
            var user = CreateTestUser();
            var logins = new List<UserLoginInfo>
            {
                new("Google", "google-key", "Google")
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ValidUserId))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GetLoginsAsync(user))
                .ReturnsAsync(logins);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.UnlinkExternalLoginAsync(ValidUserId, "GitHub"); // Not linked

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("is not linked");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task RegisterAsync_WhitespaceOnlyInput_TreatsAsEmpty()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "   ",
                Username = "   ",
                Password = "   "
            };

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeFalse();
            // Should fail on first validation
        }

        [Fact]
        public async Task LoginAsync_EmailWithExtraSpaces_TrimsCorrectly()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "  test@example.com  ", Password = ValidPassword };
            var user = CreateTestUser();

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser> { user }.AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, ValidPassword))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.ResetAccessFailedCountAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.HasPasswordAsync(user))
                .ReturnsAsync(true);

            _userManagerMock.Object.Options.SignIn.RequireConfirmedEmail = false;

            _tokenServiceMock
                .Setup(x => x.GenerateTokenPairAsync(user, ValidIpAddress))
                .ReturnsAsync((ValidToken, ValidRefreshToken, DateTime.UtcNow.AddDays(7)));

            // Act
            var result = await _sut.LoginAsync(loginDto, ValidIpAddress);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_ValidUsernameWithUnderscores_Succeeds()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = ValidEmail,
                Username = "test_user_123",
                Password = ValidPassword
            };

            SetupUserManagerNormalization();
            _userManagerMock
                .Setup(x => x.Users)
                .Returns(new List<AppUser>().AsQueryable().BuildMockDbSet().Object);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), ValidPassword))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), AppConstant.Roles.Member))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock
                .Setup(x => x.CreateToken(It.IsAny<AppUser>()))
                .ReturnsAsync(ValidToken);

            // Act
            var result = await _sut.RegisterAsync(registerDto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Username.Should().Be("test_user_123");
        }

        #endregion
    }

    #region Mock DbSet Extensions

    public static class MockDbSetExtensions
    {
        public static Mock<Microsoft.EntityFrameworkCore.DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(name: nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(System.Linq.Expressions.Expression) })!
                .MakeGenericMethod(resultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    #endregion
}
