using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TalentVerse.WebAPI.Data.Enums;

namespace TalentVerse.WebAPI.Data.Entities
{
    public class VerificationRequest
    {
        [Key]
        public long VerificationRequestId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        [Required]
        [MaxLength(2048)]
        public string DocumentUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DocumentPublicId { get; set; }

        [Required]
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedByUserId { get; set; }

        [ForeignKey("ReviewedByUserId")]
        public virtual AppUser? ReviewedBy { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}
