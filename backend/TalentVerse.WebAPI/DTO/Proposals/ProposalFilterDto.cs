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
        /// Search query - searches in username, offered skill name, and received skill name
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Sort field: "UpdatedAt" (default), "CreatedAt", "Status"
        /// </summary>
        public string? SortBy { get; set; } = "UpdatedAt";

        /// <summary>
        /// Sort order: "desc" (default) or "asc"
        /// </summary>
        public string? SortOrder { get; set; } = "desc";

        /// <summary>
        /// Filter proposals created on or after this date
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Filter proposals created on or before this date
        /// </summary>
        public DateTime? DateTo { get; set; }

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
