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
    }
}
