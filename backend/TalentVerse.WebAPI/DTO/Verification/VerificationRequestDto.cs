namespace TalentVerse.WebAPI.DTO.Verification
{
    public class VerificationRequestDto
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? UserProfilePictureUrl { get; set; }
        public string DocumentUrl { get; set; } = string.Empty;
        public string? DocumentPublicId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByUserName { get; set; }
        public string? AdminNotes { get; set; }
        public string? RejectionReason { get; set; }
    }
}
