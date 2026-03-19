using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class UserBadge
    {
        [Key]
        public long UserBadgeId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [Required]
        public int BadgeId { get; set; }

        [ForeignKey("BadgeId")]
        public virtual Badge Badge { get; set; } = null!;

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    }
}
