namespace TalentVerse.WebAPI.DTO.Messages
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ProposalId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string? SenderProfilePicture { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsOwnMessage { get; set; }
    }
}
