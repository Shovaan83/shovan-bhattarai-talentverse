namespace TalentVerse.WebAPI.DTO.Marketplace;

public class PublicSkillDto
{
    public int Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
    public string? Description { get; set; }
    public string SkillType { get; set; } = string.Empty; // "Offered" or "Wanted"
}

// Internal DTO for Dapper mapping
public class SkillQueryResult
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
    public string? Description { get; set; }
    public string SkillType { get; set; } = string.Empty;
}

// Internal DTO for swap count mapping
public class SwapCountResult
{
    public string UserId { get; set; } = string.Empty;
    public int Count { get; set; }
}

// Internal DTO for rating mapping
public class RatingResult
{
    public string UserId { get; set; } = string.Empty;
    public double AverageRating { get; set; }
}
