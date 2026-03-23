namespace TalentVerse.WebAPI.DTO.Admin;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsBanned { get; set; }
    public decimal CreditBalance { get; set; }
    public int SkillCount { get; set; }
    public int CompletedSwaps { get; set; }
    public string? Location { get; set; }
}

public class AdminUserListDto
{
    public List<AdminUserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UpdateUserStatusDto
{
    public string Action { get; set; } = string.Empty; // "Suspend", "Unsuspend", "Ban"
    public string? Reason { get; set; }
}
