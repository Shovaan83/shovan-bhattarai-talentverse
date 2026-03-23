namespace TalentVerse.WebAPI.DTO.Credits
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public decimal CreditBalance { get; set; }
        public int CompletedSwaps { get; set; }
        public int BadgeCount { get; set; }
    }
}
