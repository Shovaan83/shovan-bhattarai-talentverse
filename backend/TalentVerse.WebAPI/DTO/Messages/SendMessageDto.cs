using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Messages
{
    public class SendMessageDto
    {
        [Required(ErrorMessage = "Proposal ID is required.")]
        public int ProposalId { get; set; }

        [Required(ErrorMessage = "Message content is required.")]
        [MaxLength(2000, ErrorMessage = "Message content cannot exceed 2000 characters.")]
        public string MessageContent { get; set; } = string.Empty;
    }
}
