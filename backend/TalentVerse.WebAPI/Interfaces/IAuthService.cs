using System.Security.Claims;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Account;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<UserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ServiceResponse<UserDto>> LoginAsync(LoginDto loginDto);

        Task<ServiceResponse<CurrentUserDto>> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
        Task<ServiceResponse<CurrentUserDto>> UpdateCurrentUserAsync(ClaimsPrincipal userPrincipal, UpdateProfileDto updateProfileDto);

        Task<ServiceResponse<string>> RequestTwoFactorCodeAsync(ClaimsPrincipal userPrincipal);
        Task<ServiceResponse<bool>> EnableTwoFactorAsync(ClaimsPrincipal userPrincipal, VerifyCodeDto verifyCodeDto);

        Task<ServiceResponse<UserDto>> LoginWith2faAsync(VerifyTwoFactorDto verifyCodeDto);

        Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        Task<ServiceResponse<UserDto>> CompleteOnboardingAsync(ClaimsPrincipal userPrincipal, CompleteOnboardingDto completeOnboardingDto);
    }
}
