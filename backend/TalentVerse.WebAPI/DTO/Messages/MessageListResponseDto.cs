namespace TalentVerse.WebAPI.DTO.Messages
{
    public class MessageListResponseDto
    {
        public List<MessageDto> Messages { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
    }
}
