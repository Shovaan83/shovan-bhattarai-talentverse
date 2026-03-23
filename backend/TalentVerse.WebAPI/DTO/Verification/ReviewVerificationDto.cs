using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Verification
{
    public class ReviewVerificationDto
    {
        [Required]
        public bool IsApproved { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }
    }
}
