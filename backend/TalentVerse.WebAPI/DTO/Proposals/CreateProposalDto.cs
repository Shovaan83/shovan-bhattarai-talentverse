using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Proposals
{
    /// <summary>
    /// DTO for creating a new proposal/swap request
    /// </summary>
    public class CreateProposalDto
    {
        /// <summary>
        /// The UserSkillId of the skill the proposer is offering (must be owned by proposer, Type=Offer)
        /// </summary>
        [Required(ErrorMessage = "Proposer skill is required")]
        public int ProposerUserSkillId { get; set; }

        /// <summary>
        /// The UserSkillId of the skill the proposer wants (must be owned by recipient, Type=Offer)
        /// </summary>
        [Required(ErrorMessage = "Recipient skill is required")]
        public int RecipientUserSkillId { get; set; }

        /// <summary>
        /// Optional message to include with the proposal
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string? Message { get; set; }

        /// <summary>
        /// Legacy net credits proposed for the swap. Used as recipient credits when directional amounts are omitted.
        /// </summary>
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Credit amount cannot be negative.")]
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Credits the recipient should pay to the proposer for the proposer's offered skill.
        /// </summary>
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Proposer credit amount cannot be negative.")]
        public decimal ProposerCreditAmount { get; set; }

        /// <summary>
        /// Credits the proposer should pay to the recipient for the requested skill.
        /// </summary>
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Recipient credit amount cannot be negative.")]
        public decimal RecipientCreditAmount { get; set; }
    }
}
