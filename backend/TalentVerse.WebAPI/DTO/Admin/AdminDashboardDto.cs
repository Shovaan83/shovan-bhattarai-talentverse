namespace TalentVerse.WebAPI.DTO.Admin;

public class AdminDashboardDto
{
    // KPI cards
    public int TotalUsers { get; set; }
    public int ActiveUsersLast30Days { get; set; }
    public int TotalSwaps { get; set; }
    public decimal TotalCreditsCirculated { get; set; }
    public int PendingVerifications { get; set; }
    public int TotalReviews { get; set; }

    // Chart data
    public List<UserGrowthPoint> UserGrowth { get; set; } = new();
    public List<TopSkillPoint> TopSkills { get; set; } = new();
    public ProposalStatsDto ProposalStats { get; set; } = new();
}

public class UserGrowthPoint
{
    public string Month { get; set; } = string.Empty; // "2026-01", "2026-02", etc.
    public int Count { get; set; }
}

public class TopSkillPoint
{
    public string SkillName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

public class ProposalStatsDto
{
    public int Pending { get; set; }
    public int Accepted { get; set; }
    public int Declined { get; set; }
    public int Completed { get; set; }
}
