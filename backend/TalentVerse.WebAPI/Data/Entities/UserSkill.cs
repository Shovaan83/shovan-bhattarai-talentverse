using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TalentVerse.WebAPI.Data.Enums;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class UserSkill
    {
        [Key]
        public int UserSkillId { get; set; }

        [Required]
        public string UserId { get; set; } 
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        [Required]
        public int SkillId { get; set; }
        [ForeignKey("SkillId")]
        public virtual Skill Skill { get; set; }

        [Required]
        public SkillType Type { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}