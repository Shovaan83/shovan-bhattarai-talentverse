namespace TalentVerse.WebAPI.DTO.Account;

/// <summary>
/// Result of external login attempt
/// </summary>
public class ExternalLoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public bool IsNewUser { get; set; }
    public bool RequiresOnboarding { get; set; }
    public bool IsTwoFactorSetupComplete { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}
