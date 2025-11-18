
using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class Skill
    {
        [Key]
        public int SkillId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }
}