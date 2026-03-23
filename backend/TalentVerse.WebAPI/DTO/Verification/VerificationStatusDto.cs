namespace TalentVerse.WebAPI.DTO.Verification
{
    public class VerificationStatusDto
    {
        public string Status { get; set; } = "None";
        public bool IsVerified { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
