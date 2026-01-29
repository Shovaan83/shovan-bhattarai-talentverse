using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Account;

public class CompleteOnboardingDto
{
    [MaxLength(500)]
    public string? Bio { get; set; }

    [Required(ErrorMessage = "Location is required to complete your profile")]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Profile picture is required to complete your profile")]
    [MaxLength(2048)]
    public string ProfilePictureUrl { get; set; } = string.Empty;

    public SocialLinksDto? SocialLinks { get; set; }
}
