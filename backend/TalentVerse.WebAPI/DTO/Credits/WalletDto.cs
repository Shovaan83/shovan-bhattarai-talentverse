namespace TalentVerse.WebAPI.DTO.Credits
{
    public class WalletDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int TotalSwapsCompleted { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
