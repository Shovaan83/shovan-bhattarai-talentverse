namespace TalentVerse.WebAPI.DTO.Admin;

// --- Admin: Browse proposals ---
public class AdminProposalDto
{
    public int ProposalId { get; set; }
    public string ProposerName { get; set; } = string.Empty;
    public string ProposerId { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public string ProposerSkill { get; set; } = string.Empty;
    public string RecipientSkill { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool ProposerConfirmed { get; set; }
    public bool RecipientConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminProposalListDto
{
    public List<AdminProposalDto> Proposals { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// --- Admin: Resolve dispute ---
public class ResolveDisputeDto
{
    public string Action { get; set; } = string.Empty; // "ForceComplete" or "ForceCancel"
    public string AdminNote { get; set; } = string.Empty;
}
