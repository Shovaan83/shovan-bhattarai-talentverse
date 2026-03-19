namespace TalentVerse.WebAPI.DTO.Credits
{
    public class TransactionListResponseDto
    {
        public IEnumerable<CreditTransactionDto> Transactions { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
