using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Account;

public class SocialLinksDto
{
    [MaxLength(2048)]
    [Url(ErrorMessage = "GitHub URL must be a valid URL")]
    public string? GitHubUrl { get; set; }

    [MaxLength(2048)]
    [Url(ErrorMessage = "LinkedIn URL must be a valid URL")]
    public string? LinkedInUrl { get; set; }

    [MaxLength(2048)]
    [Url(ErrorMessage = "Twitter URL must be a valid URL")]
    public string? TwitterUrl { get; set; }

    [MaxLength(2048)]
    [Url(ErrorMessage = "Website URL must be a valid URL")]
    public string? WebsiteUrl { get; set; }
}
