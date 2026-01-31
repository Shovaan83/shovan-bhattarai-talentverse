namespace TalentVerse.WebAPI.DTO.Marketplace;

public class SkillBrowseDto
{
    public string SkillName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public double AverageProficiency { get; set; }
}
