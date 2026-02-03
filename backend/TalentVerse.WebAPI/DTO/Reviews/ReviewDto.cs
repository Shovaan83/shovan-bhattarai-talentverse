namespace TalentVerse.WebAPI.DTO.Reviews;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int ProposalId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerUsername { get; set; } = string.Empty;
    public string ReviewerProfilePictureUrl { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public string RevieweeUsername { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
