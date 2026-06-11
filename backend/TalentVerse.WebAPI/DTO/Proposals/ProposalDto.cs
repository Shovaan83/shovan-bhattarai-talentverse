namespace TalentVerse.WebAPI.DTO.Proposals
{
    /// <summary>
    /// Full proposal details for single proposal view
    /// </summary>
    public class ProposalDto
    {
        public int ProposalId { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal ProposerCreditAmount { get; set; }
        public decimal RecipientCreditAmount { get; set; }
        public decimal NetCreditAmount { get; set; }
        public string NetCreditReceiverUserId { get; set; } = string.Empty;

        // Proposer info
        public string ProposerId { get; set; } = string.Empty;
        public string ProposerUsername { get; set; } = string.Empty;
        public string? ProposerProfilePicture { get; set; }

        // Recipient info
        public string RecipientId { get; set; } = string.Empty;
        public string RecipientUsername { get; set; } = string.Empty;
        public string? RecipientProfilePicture { get; set; }

        // Skill being offered by proposer
        public int ProposerUserSkillId { get; set; }
        public string ProposerSkillName { get; set; } = string.Empty;
        public string ProposerSkillCategory { get; set; } = string.Empty;
        public string? ProposerSkillDescription { get; set; }

        // Skill being requested from recipient
        public int RecipientUserSkillId { get; set; }
        public string RecipientSkillName { get; set; } = string.Empty;
        public string RecipientSkillCategory { get; set; } = string.Empty;
        public string? RecipientSkillDescription { get; set; }

        // Status info
        public string Status { get; set; } = string.Empty;
        public bool ProposerConfirmed { get; set; }
        public bool RecipientConfirmed { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties for frontend convenience
        public bool CanAccept { get; set; }
        public bool CanDecline { get; set; }
        public bool CanCancel { get; set; }
        public bool CanConfirmCompletion { get; set; }
        public bool CanCounteroffer { get; set; }

        public List<ProposalCounterofferDto> Counteroffers { get; set; } = new();
    }
}
