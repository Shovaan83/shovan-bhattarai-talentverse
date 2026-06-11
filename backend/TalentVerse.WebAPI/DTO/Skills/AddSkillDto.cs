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

        [Required]
        [Range(1, 5, ErrorMessage = "Proficiency level must be between 1 and 5")]
        public int ProficiencyLevel { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
