using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class Badge
    {
        [Key]
        public int BadgeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string IconKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Tier { get; set; } = "Bronze"; // Bronze | Silver | Gold

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Engagement | Skill | Economy | Milestone

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditReward { get; set; } = 0;

        public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    }
}
