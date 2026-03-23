namespace TalentVerse.WebAPI.DTO.Verification
{
    public class AdminVerificationListDto
    {
        public List<VerificationRequestDto> Requests { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
