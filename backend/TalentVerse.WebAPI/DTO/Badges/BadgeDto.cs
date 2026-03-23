namespace TalentVerse.WebAPI.DTO.Badges
{
    public class BadgeDto
    {
        public int BadgeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IconKey { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal CreditReward { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedAt { get; set; }
    }
}
