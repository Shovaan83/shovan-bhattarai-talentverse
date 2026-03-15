namespace TalentVerse.WebAPI.DTO.Messages
{
    public class ConversationDto
    {
        public int ProposalId { get; set; }
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherUsername { get; set; } = string.Empty;
        public string? OtherUserProfilePicture { get; set; }
        public string OfferingSkillName { get; set; } = string.Empty;
        public string ReceivingSkillName { get; set; } = string.Empty;
        public string ProposalStatus { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
