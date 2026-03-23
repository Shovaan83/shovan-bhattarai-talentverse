namespace TalentVerse.WebAPI.DTO.Credits
{
    public class TransactionFilterDto
    {
        public string? Type { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
