namespace TalentVerse.WebAPI.DTO.Reviews;

public class UserReputationDto
{
    public string UserId { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedSwaps { get; set; }
    public bool HasMinimumReviews => TotalReviews >= 3; // Minimum 3 reviews to display reputation
}
