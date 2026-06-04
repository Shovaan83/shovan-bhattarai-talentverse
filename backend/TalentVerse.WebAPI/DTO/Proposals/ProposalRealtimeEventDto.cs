namespace TalentVerse.WebAPI.DTO.Proposals
{
    public class ProposalRealtimeEventDto
    {
        public int ProposalId { get; set; }
        public string ProposerId { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
        public string ProposerUsername { get; set; } = string.Empty;
        public string RecipientUsername { get; set; } = string.Empty;
        public string ProposerProfilePicture { get; set; } = string.Empty;
        public string RecipientProfilePicture { get; set; } = string.Empty;
        public string OfferingSkillName { get; set; } = string.Empty;
        public string ReceivingSkillName { get; set; } = string.Empty;
        public decimal CreditAmount { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ActorUserId { get; set; } = string.Empty;
        public string? ActorUsername { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}