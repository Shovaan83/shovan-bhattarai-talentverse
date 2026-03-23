namespace TalentVerse.WebAPI.DTO.Marketplace;

public class UserSearchDto
{
    public string? Query { get; set; }
    public string? SkillName { get; set; }
    public string? SkillType { get; set; } // "Offered" or "Wanted"
    public string? Category { get; set; }
    public int? MinProficiency { get; set; }
    public int? MaxProficiency { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
