using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Account
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string? Bio { get; set; }
        public string Token { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsTwoFactorRequired { get; set; } = false;
        public bool IsProfileComplete { get; set; } = false;
        public bool IsTwoFactorSetupComplete { get; set; } = false;
        public bool HasPassword { get; set; } = true;
        public string? Location { get; set; }
        public string? GitHubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? WebsiteUrl { get; set; }
    }
}
