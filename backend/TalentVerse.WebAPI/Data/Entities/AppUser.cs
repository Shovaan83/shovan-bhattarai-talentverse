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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

        public virtual ICollection<Proposal> SentProposals { get; set; } = new List<Proposal>();

        public virtual ICollection<Proposal> ReceivedProposals { get; set; } = new List<Proposal>();

        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

        public virtual ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();

        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();

        public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();
    }
}