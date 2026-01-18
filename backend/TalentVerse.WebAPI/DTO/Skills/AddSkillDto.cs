using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Skills
{
    public class AddSkillDto
    {
        [Required]
        public string SkillName { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(0, 1)]
        public int Type { get; set; } // 0 = Offered, 1 = Wanted

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
