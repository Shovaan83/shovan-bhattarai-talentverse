namespace TalentVerse.WebAPI.DTO.Marketplace;

public class PublicUserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public DateTime JoinedAt { get; set; }
    public List<PublicSkillDto> OfferedSkills { get; set; } = new();
    public List<PublicSkillDto> WantedSkills { get; set; } = new();
    public int CompletedSwaps { get; set; }
    public double? AverageRating { get; set; }
}
