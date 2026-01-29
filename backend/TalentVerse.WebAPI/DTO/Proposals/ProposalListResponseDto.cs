namespace TalentVerse.WebAPI.DTO.Proposals
{
    /// <summary>
    /// Paginated response for proposal lists
    /// </summary>
    public class ProposalListResponseDto
    {
        public List<ProposalListDto> Proposals { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
