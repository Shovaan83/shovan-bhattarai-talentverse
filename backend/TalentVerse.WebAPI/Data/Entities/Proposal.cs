using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TalentVerse.WebAPI.Data.Enums;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class Proposal
    {
        [Key]
        public int ProposalId { get; set; }

        [Required]
        public string ProposerId { get; set; } 
        [ForeignKey("ProposerId")]
        public virtual AppUser Proposer { get; set; }

        [Required]
        public string RecipientId { get; set; }
        [ForeignKey("RecipientId")]
        public virtual AppUser Recipient { get; set; }

        [Required]
        public int ProposerUserSkillId { get; set; }
        [ForeignKey("ProposerUserSkillId")]
        public virtual UserSkill ProposerUserSkill { get; set; }

        [Required]
        public int RecipientUserSkillId { get; set; }
        [ForeignKey("RecipientUserSkillId")]
        public virtual UserSkill RecipientUserSkill { get; set; }

        [Required]
        public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}