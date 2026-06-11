using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Proposals
{
    public class CreateCounterofferDto
    {
        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Credit amount cannot be negative.")]
        public decimal CreditAmount { get; set; }

        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Proposer credit amount cannot be negative.")]
        public decimal ProposerCreditAmount { get; set; }

        [Range(typeof(decimal), "0", "999999.99", ErrorMessage = "Recipient credit amount cannot be negative.")]
        public decimal RecipientCreditAmount { get; set; }

        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string? Message { get; set; }
    }
}
