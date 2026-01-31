namespace TalentVerse.WebAPI.DTO.Proposals
{
    /// <summary>
    /// Filter options for querying proposals
    /// </summary>
    public class ProposalFilterDto
    {
        /// <summary>
        /// Filter by proposal direction: "sent", "received", or null for all
        /// </summary>
        public string? Direction { get; set; }

        /// <summary>
        /// Filter by status: "pending", "accepted", "rejected", "completed", "cancelled", or null for all
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Items per page (max 50)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
