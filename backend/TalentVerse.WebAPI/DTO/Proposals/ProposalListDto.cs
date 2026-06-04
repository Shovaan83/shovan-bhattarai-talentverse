namespace TalentVerse.WebAPI.DTO.Proposals
{
    /// <summary>
    /// Lightweight proposal DTO for list views and dashboards
    /// </summary>
    public class ProposalListDto
    {
        public int ProposalId { get; set; }
        public decimal CreditAmount { get; set; }

        // Other party info (the person you're swapping with)
        public string OtherUserId { get; set; } = string.Empty;
        public string OtherUsername { get; set; } = string.Empty;
        public string? OtherProfilePicture { get; set; }

        // What you're offering
        public string OfferingSkillName { get; set; } = string.Empty;

        // What you're getting
        public string ReceivingSkillName { get; set; } = string.Empty;

        // Status
        public string Status { get; set; } = string.Empty;
        public bool ProposerConfirmed { get; set; }
        public bool RecipientConfirmed { get; set; }

        // Is current user the proposer or recipient?
        public bool IsProposer { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
