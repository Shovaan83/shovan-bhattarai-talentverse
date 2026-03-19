using TalentVerse.WebAPI.Data.Enums;

namespace TalentVerse.WebAPI.DTO.Credits
{
    public class CreditTransactionDto
    {
        public long TransactionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public string TypeLabel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public long? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }
}
