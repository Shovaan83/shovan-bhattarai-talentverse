namespace TalentVerse.WebAPI.DTO.Admin;

// --- User-facing report submission ---
public class ReportContentDto
{
    public string ContentType { get; set; } = string.Empty; // "Skill" or "Review"
    public int ContentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

// --- Admin: Flagged content queue ---
public class FlaggedContentDto
{
    public int ReportId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int ContentId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Inline content preview
    public string? ContentOwnerName { get; set; }
    public string? ContentPreview { get; set; } // Skill name or review comment snippet
    public int? Rating { get; set; } // For reviews only
}

public class FlaggedContentListDto
{
    public List<FlaggedContentDto> Reports { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// --- Admin: Browse skills ---
public class AdminSkillDto
{
    public int UserSkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Type { get; set; } // 0 = Offer, 1 = Want
    public string? Description { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminSkillListDto
{
    public List<AdminSkillDto> Skills { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// --- Admin: Browse reviews ---
public class AdminReviewDto
{
    public int ReviewId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int ProposalId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminReviewListDto
{
    public List<AdminReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// --- Admin: Remove content ---
public class RemoveContentDto
{
    public string Reason { get; set; } = string.Empty;
}
