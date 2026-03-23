using System.ComponentModel.DataAnnotations;

namespace TalentVerse.WebAPI.DTO.Verification
{
    public class SubmitVerificationRequestDto
    {
        [Required]
        [MaxLength(2048)]
        public string DocumentUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DocumentPublicId { get; set; }
    }
}
