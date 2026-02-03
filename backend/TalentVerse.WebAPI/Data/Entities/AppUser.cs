using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class AppUser : IdentityUser
    {
        [MaxLength(500)] 
        public string? Bio { get; set; }

        [MaxLength(2048)] 
        public string? ProfilePictureURL { get; set; }

    [MaxLength(2048)]
    public string? CoverPhotoUrl { get; set; }

    public bool IsProfileComplete { get; set; } = false;

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(2048)]
    public string? GitHubUrl { get; set; }

    [MaxLength(2048)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(2048)]
    public string? TwitterUrl { get; set; }

    [MaxLength(2048)]
    public string? WebsiteUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }

    // ⭐ Refresh Token fields for cookie-based authentication
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // ⭐ Tracks if user has completed 2FA setup wizard (for email/password users)
    public bool IsTwoFactorSetupComplete { get; set; } = false;

        public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

        public virtual ICollection<Proposal> SentProposals { get; set; } = new List<Proposal>();

        public virtual ICollection<Proposal> ReceivedProposals { get; set; } = new List<Proposal>();

        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

        public virtual ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();

        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();

        public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();
    }
}