using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class ProposalCounteroffer
    {
        [Key]
        public long ProposalCounterofferId { get; set; }

        [Required]
        public int ProposalId { get; set; }
        [ForeignKey("ProposalId")]
        public virtual Proposal Proposal { get; set; }

        [Required]
        public string OfferedByUserId { get; set; } = string.Empty;
        [ForeignKey("OfferedByUserId")]
        public virtual AppUser OfferedByUser { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProposerCreditAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RecipientCreditAmount { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
