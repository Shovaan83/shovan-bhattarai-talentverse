using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ProposalId { get; set; } 
        [ForeignKey("ProposalId")]
        public virtual Proposal Proposal { get; set; }

        [Required]
        public string SenderId { get; set; } 
        [ForeignKey("SenderId")]
        public virtual AppUser Sender { get; set; }

        [Required]
        public string MessageContent { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}