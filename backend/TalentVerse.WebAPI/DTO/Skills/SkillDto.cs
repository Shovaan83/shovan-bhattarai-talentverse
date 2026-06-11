namespace TalentVerse.WebAPI.DTO.Skills
{
    public class SkillDto
    {
        public int UserSkillId { get; set; } //the id of the link

        public string SkillName { get; set; }

        public string Category { get; set; }

        public string Type { get; set; } // offer or want for the frontend

        public int ProficiencyLevel { get; set; }

        public string? Description { get; set; }
    }
}
