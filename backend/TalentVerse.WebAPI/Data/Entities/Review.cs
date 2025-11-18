using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ProposalId { get; set; }
        [ForeignKey("ProposalId")]
        public virtual Proposal Proposal { get; set; }

        [Required]
        public string ReviewerId { get; set; }
        [ForeignKey("ReviewerId")]
        public virtual AppUser Reviewer { get; set; }

        [Required]
        public string RevieweeId { get; set; }
        [ForeignKey("RevieweeId")]
        public virtual AppUser Reviewee { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}