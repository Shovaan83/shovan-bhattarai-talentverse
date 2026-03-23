using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.DTO.Admin;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly DapperContext _dapperContext;
    private readonly ILogger<AdminRepository> _logger;

    public AdminRepository(DapperContext dapperContext, ILogger<AdminRepository> logger)
    {
        _dapperContext = dapperContext;
        _logger = logger;
    }

    public async Task<(List<AdminUserDto> Users, int TotalCount)> SearchUsersAsync(string? query, int page, int pageSize)
    {
        using var connection = _dapperContext.CreateConnection();

        var offset = (page - 1) * pageSize;
        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var whereClause = "1=1";
        if (!string.IsNullOrWhiteSpace(query))
        {
            whereClause = "(LOWER(u.\"UserName\") LIKE @Query OR LOWER(u.\"Email\") LIKE @Query)";
            parameters.Add("Query", $"%{query.ToLower()}%");
        }

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM ""AspNetUsers"" u
            WHERE {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Main query
        var sql = $@"
            SELECT
                u.""Id"",
                u.""UserName"",
                u.""Email"",
                u.""ProfilePictureURL"" AS ProfilePictureUrl,
                u.""CreatedAt"",
                u.""IsIdentityVerified"" AS IsVerified,
                CASE WHEN u.""LockoutEnd"" IS NOT NULL AND u.""LockoutEnd"" > NOW() AND u.""DeletedAt"" IS NULL THEN true ELSE false END AS IsSuspended,
                CASE WHEN u.""DeletedAt"" IS NOT NULL THEN true ELSE false END AS IsBanned,
                u.""CreditBalance"",
                u.""Location"",
                COALESCE(sk.cnt, 0) AS SkillCount,
                COALESCE(sw.cnt, 0) AS CompletedSwaps
            FROM ""AspNetUsers"" u
            LEFT JOIN (
                SELECT ""UserId"", COUNT(*) AS cnt FROM ""UserSkills"" GROUP BY ""UserId""
            ) sk ON sk.""UserId"" = u.""Id""
            LEFT JOIN (
                SELECT uid, COUNT(*) AS cnt FROM (
                    SELECT ""ProposerId"" AS uid FROM ""Proposals"" WHERE ""Status"" = 3
                    UNION ALL
                    SELECT ""RecipientId"" AS uid FROM ""Proposals"" WHERE ""Status"" = 3
                ) x GROUP BY uid
            ) sw ON sw.uid = u.""Id""
            WHERE {whereClause}
            ORDER BY u.""CreatedAt"" DESC
            OFFSET @Offset LIMIT @PageSize";

        var users = (await connection.QueryAsync<AdminUserDto>(sql, parameters)).ToList();

        return (users, totalCount);
    }

    public async Task<AdminDashboardDto> GetDashboardDataAsync()
    {
        using var connection = _dapperContext.CreateConnection();

        var dto = new AdminDashboardDto();

        // KPI — total users
        dto.TotalUsers = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM ""AspNetUsers"" WHERE ""DeletedAt"" IS NULL");

        // KPI — active users (logged in / created within 30 days)
        dto.ActiveUsersLast30Days = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM ""AspNetUsers""
              WHERE ""DeletedAt"" IS NULL
              AND ""CreatedAt"" >= NOW() - INTERVAL '30 days'");

        // KPI — total completed swaps
        dto.TotalSwaps = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM ""Proposals"" WHERE ""Status"" = 3");

        // KPI — total credits circulated
        dto.TotalCreditsCirculated = await connection.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(""Amount""), 0) FROM ""CreditTransactions"" WHERE ""Amount"" > 0");

        // KPI — pending verifications
        dto.PendingVerifications = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM ""VerificationRequests"" WHERE ""Status"" = 0");

        // KPI — total reviews
        dto.TotalReviews = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM ""Reviews""");

        // Chart: User growth last 12 months
        dto.UserGrowth = (await connection.QueryAsync<UserGrowthPoint>(@"
            SELECT TO_CHAR(d.month, 'YYYY-MM') AS Month,
                   COUNT(u.""Id"") AS Count
            FROM generate_series(
                DATE_TRUNC('month', NOW()) - INTERVAL '11 months',
                DATE_TRUNC('month', NOW()),
                '1 month'
            ) d(month)
            LEFT JOIN ""AspNetUsers"" u
                ON DATE_TRUNC('month', u.""CreatedAt"") = d.month
                AND u.""DeletedAt"" IS NULL
            GROUP BY d.month
            ORDER BY d.month")).ToList();

        // Chart: Top 10 skills
        dto.TopSkills = (await connection.QueryAsync<TopSkillPoint>(@"
            SELECT s.""SkillName"" AS SkillName,
                   COUNT(DISTINCT us.""UserId"") AS UserCount
            FROM ""UserSkills"" us
            INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
            GROUP BY s.""SkillName""
            ORDER BY UserCount DESC
            LIMIT 10")).ToList();

        // Chart: Proposal status distribution
        var proposalRows = await connection.QueryAsync<(int Status, int Count)>(@"
            SELECT ""Status"", COUNT(*) AS Count
            FROM ""Proposals""
            GROUP BY ""Status""");

        foreach (var row in proposalRows)
        {
            switch (row.Status)
            {
                case 0: dto.ProposalStats.Pending = row.Count; break;
                case 1: dto.ProposalStats.Accepted = row.Count; break;
                case 2: dto.ProposalStats.Declined = row.Count; break;
                case 3: dto.ProposalStats.Completed = row.Count; break;
            }
        }

        return dto;
    }

    // ───────── Content Moderation ─────────

    public async Task<int> CreateReportAsync(string reporterId, ReportContentDto dto)
    {
        using var connection = _dapperContext.CreateConnection();

        var sql = @"
            INSERT INTO ""ContentReports"" (""ReporterId"", ""ContentType"", ""ContentId"", ""Reason"", ""Status"", ""CreatedAt"")
            VALUES (@ReporterId, @ContentType, @ContentId, @Reason, 'Pending', NOW())
            RETURNING ""Id""";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            ReporterId = reporterId,
            dto.ContentType,
            dto.ContentId,
            dto.Reason
        });
    }

    public async Task<(List<FlaggedContentDto> Reports, int TotalCount)> GetFlaggedContentAsync(int page, int pageSize)
    {
        using var connection = _dapperContext.CreateConnection();
        var offset = (page - 1) * pageSize;

        var countSql = @"SELECT COUNT(*) FROM ""ContentReports"" WHERE ""Status"" = 'Pending'";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT
                cr.""Id"" AS ReportId,
                cr.""ContentType"",
                cr.""ContentId"",
                reporter.""UserName"" AS ReporterName,
                cr.""Reason"",
                cr.""CreatedAt"",
                CASE
                    WHEN cr.""ContentType"" = 'Skill' THEN owner_sk.""UserName""
                    WHEN cr.""ContentType"" = 'Review' THEN reviewer.""UserName""
                    ELSE NULL
                END AS ContentOwnerName,
                CASE
                    WHEN cr.""ContentType"" = 'Skill' THEN sk_name.""SkillName""
                    WHEN cr.""ContentType"" = 'Review' THEN LEFT(rv.""Comment"", 80)
                    ELSE NULL
                END AS ContentPreview,
                rv.""Rating""
            FROM ""ContentReports"" cr
            INNER JOIN ""AspNetUsers"" reporter ON reporter.""Id"" = cr.""ReporterId""
            LEFT JOIN ""UserSkills"" us ON cr.""ContentType"" = 'Skill' AND us.""UserSkillId"" = cr.""ContentId""
            LEFT JOIN ""Skills"" sk_name ON us.""SkillId"" = sk_name.""SkillId""
            LEFT JOIN ""AspNetUsers"" owner_sk ON us.""UserId"" = owner_sk.""Id""
            LEFT JOIN ""Reviews"" rv ON cr.""ContentType"" = 'Review' AND rv.""ReviewId"" = cr.""ContentId""
            LEFT JOIN ""AspNetUsers"" reviewer ON rv.""ReviewerId"" = reviewer.""Id""
            WHERE cr.""Status"" = 'Pending'
            ORDER BY cr.""CreatedAt"" DESC
            OFFSET @Offset LIMIT @PageSize";

        var reports = (await connection.QueryAsync<FlaggedContentDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();
        return (reports, totalCount);
    }

    public async Task<(List<AdminSkillDto> Skills, int TotalCount)> SearchSkillsAsync(string? query, int page, int pageSize)
    {
        using var connection = _dapperContext.CreateConnection();
        var offset = (page - 1) * pageSize;
        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var whereClause = "1=1";
        if (!string.IsNullOrWhiteSpace(query))
        {
            whereClause = "(LOWER(s.\"SkillName\") LIKE @Query OR LOWER(u.\"UserName\") LIKE @Query)";
            parameters.Add("Query", $"%{query.ToLower()}%");
        }

        var countSql = $@"
            SELECT COUNT(*)
            FROM ""UserSkills"" us
            INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
            INNER JOIN ""AspNetUsers"" u ON us.""UserId"" = u.""Id""
            WHERE {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT
                us.""UserSkillId"",
                s.""SkillName"",
                s.""Category"",
                us.""Type"",
                us.""Description"",
                u.""UserName"",
                u.""Id"" AS UserId,
                us.""CreatedAt""
            FROM ""UserSkills"" us
            INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
            INNER JOIN ""AspNetUsers"" u ON us.""UserId"" = u.""Id""
            WHERE {whereClause}
            ORDER BY us.""CreatedAt"" DESC
            OFFSET @Offset LIMIT @PageSize";

        var skills = (await connection.QueryAsync<AdminSkillDto>(sql, parameters)).ToList();
        return (skills, totalCount);
    }

    public async Task<(List<AdminReviewDto> Reviews, int TotalCount)> SearchReviewsAsync(string? query, int page, int pageSize)
    {
        using var connection = _dapperContext.CreateConnection();
        var offset = (page - 1) * pageSize;
        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var whereClause = "1=1";
        if (!string.IsNullOrWhiteSpace(query))
        {
            whereClause = "(LOWER(reviewer.\"UserName\") LIKE @Query OR LOWER(reviewee.\"UserName\") LIKE @Query OR LOWER(r.\"Comment\") LIKE @Query)";
            parameters.Add("Query", $"%{query.ToLower()}%");
        }

        var countSql = $@"
            SELECT COUNT(*)
            FROM ""Reviews"" r
            INNER JOIN ""AspNetUsers"" reviewer ON r.""ReviewerId"" = reviewer.""Id""
            INNER JOIN ""AspNetUsers"" reviewee ON r.""RevieweeId"" = reviewee.""Id""
            WHERE {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var sql = $@"
            SELECT
                r.""ReviewId"",
                reviewer.""UserName"" AS ReviewerName,
                reviewee.""UserName"" AS RevieweeName,
                r.""Rating"",
                r.""Comment"",
                r.""ProposalId"",
                r.""CreatedAt""
            FROM ""Reviews"" r
            INNER JOIN ""AspNetUsers"" reviewer ON r.""ReviewerId"" = reviewer.""Id""
            INNER JOIN ""AspNetUsers"" reviewee ON r.""RevieweeId"" = reviewee.""Id""
            WHERE {whereClause}
            ORDER BY r.""CreatedAt"" DESC
            OFFSET @Offset LIMIT @PageSize";

        var reviews = (await connection.QueryAsync<AdminReviewDto>(sql, parameters)).ToList();
        return (reviews, totalCount);
    }

    public async Task<bool> DeleteUserSkillAsync(int userSkillId)
    {
        using var connection = _dapperContext.CreateConnection();
        var affected = await connection.ExecuteAsync(
            @"DELETE FROM ""UserSkills"" WHERE ""UserSkillId"" = @Id", new { Id = userSkillId });
        return affected > 0;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId)
    {
        using var connection = _dapperContext.CreateConnection();
        var affected = await connection.ExecuteAsync(
            @"DELETE FROM ""Reviews"" WHERE ""ReviewId"" = @Id", new { Id = reviewId });
        return affected > 0;
    }

    public async Task<bool> ResolveReportsForContentAsync(string contentType, int contentId, string adminId)
    {
        using var connection = _dapperContext.CreateConnection();
        var affected = await connection.ExecuteAsync(@"
            UPDATE ""ContentReports""
            SET ""Status"" = 'Resolved', ""ResolvedAt"" = NOW(), ""ResolvedByAdminId"" = @AdminId
            WHERE ""ContentType"" = @ContentType AND ""ContentId"" = @ContentId AND ""Status"" = 'Pending'",
            new { ContentType = contentType, ContentId = contentId, AdminId = adminId });
        return affected >= 0;
    }

    public async Task<bool> DismissReportAsync(int reportId, string adminId)
    {
        using var connection = _dapperContext.CreateConnection();
        var affected = await connection.ExecuteAsync(@"
            UPDATE ""ContentReports""
            SET ""Status"" = 'Dismissed', ""ResolvedAt"" = NOW(), ""ResolvedByAdminId"" = @AdminId
            WHERE ""Id"" = @ReportId AND ""Status"" = 'Pending'",
            new { ReportId = reportId, AdminId = adminId });
        return affected > 0;
    }

    // ───────── Dispute Resolution ─────────

    public async Task<(List<AdminProposalDto> Proposals, int TotalCount)> SearchProposalsAsync(string? query, int? status, int page, int pageSize)
    {
        using var connection = _dapperContext.CreateConnection();
        var offset = (page - 1) * pageSize;
        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(query))
        {
            conditions.Add("(LOWER(proposer.\"UserName\") LIKE @Query OR LOWER(recipient.\"UserName\") LIKE @Query)");
            parameters.Add("Query", $"%{query.ToLower()}%");
        }
        if (status.HasValue)
        {
            conditions.Add("p.\"Status\" = @Status");
            parameters.Add("Status", status.Value);
        }

        var whereClause = conditions.Count > 0 ? string.Join(" AND ", conditions) : "1=1";

        var countSql = $@"
            SELECT COUNT(*)
            FROM ""Proposals"" p
            INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
            INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
            WHERE {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var statusCase = @"
            CASE p.""Status""
                WHEN 0 THEN 'Pending'
                WHEN 1 THEN 'Accepted'
                WHEN 2 THEN 'Rejected'
                WHEN 3 THEN 'Completed'
                WHEN 4 THEN 'Cancelled'
                ELSE 'Unknown'
            END";

        var sql = $@"
            SELECT
                p.""ProposalId"",
                proposer.""UserName"" AS ProposerName,
                proposer.""Id"" AS ProposerId,
                recipient.""UserName"" AS RecipientName,
                recipient.""Id"" AS RecipientId,
                ps.""SkillName"" AS ProposerSkill,
                rs.""SkillName"" AS RecipientSkill,
                {statusCase} AS Status,
                p.""ProposerConfirmed"",
                p.""RecipientConfirmed"",
                p.""CreatedAt"",
                p.""UpdatedAt""
            FROM ""Proposals"" p
            INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
            INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
            LEFT JOIN ""UserSkills"" pus ON p.""ProposerUserSkillId"" = pus.""UserSkillId""
            LEFT JOIN ""Skills"" ps ON pus.""SkillId"" = ps.""SkillId""
            LEFT JOIN ""UserSkills"" rus ON p.""RecipientUserSkillId"" = rus.""UserSkillId""
            LEFT JOIN ""Skills"" rs ON rus.""SkillId"" = rs.""SkillId""
            WHERE {whereClause}
            ORDER BY p.""UpdatedAt"" DESC
            OFFSET @Offset LIMIT @PageSize";

        var proposals = (await connection.QueryAsync<AdminProposalDto>(sql, parameters)).ToList();
        return (proposals, totalCount);
    }

    public async Task<AdminProposalDto?> GetProposalForAdminAsync(int proposalId)
    {
        using var connection = _dapperContext.CreateConnection();

        var statusCase = @"
            CASE p.""Status""
                WHEN 0 THEN 'Pending'
                WHEN 1 THEN 'Accepted'
                WHEN 2 THEN 'Rejected'
                WHEN 3 THEN 'Completed'
                WHEN 4 THEN 'Cancelled'
                ELSE 'Unknown'
            END";

        var sql = $@"
            SELECT
                p.""ProposalId"",
                proposer.""UserName"" AS ProposerName,
                proposer.""Id"" AS ProposerId,
                recipient.""UserName"" AS RecipientName,
                recipient.""Id"" AS RecipientId,
                ps.""SkillName"" AS ProposerSkill,
                rs.""SkillName"" AS RecipientSkill,
                {statusCase} AS Status,
                p.""ProposerConfirmed"",
                p.""RecipientConfirmed"",
                p.""CreatedAt"",
                p.""UpdatedAt""
            FROM ""Proposals"" p
            INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
            INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
            LEFT JOIN ""UserSkills"" pus ON p.""ProposerUserSkillId"" = pus.""UserSkillId""
            LEFT JOIN ""Skills"" ps ON pus.""SkillId"" = ps.""SkillId""
            LEFT JOIN ""UserSkills"" rus ON p.""RecipientUserSkillId"" = rus.""UserSkillId""
            LEFT JOIN ""Skills"" rs ON rus.""SkillId"" = rs.""SkillId""
            WHERE p.""ProposalId"" = @Id";

        return await connection.QueryFirstOrDefaultAsync<AdminProposalDto>(sql, new { Id = proposalId });
    }

    public async Task<bool> ForceUpdateProposalStatusAsync(int proposalId, int status)
    {
        using var connection = _dapperContext.CreateConnection();
        var affected = await connection.ExecuteAsync(@"
            UPDATE ""Proposals""
            SET ""Status"" = @Status,
                ""ProposerConfirmed"" = CASE WHEN @Status = 3 THEN true ELSE ""ProposerConfirmed"" END,
                ""RecipientConfirmed"" = CASE WHEN @Status = 3 THEN true ELSE ""RecipientConfirmed"" END,
                ""UpdatedAt"" = NOW()
            WHERE ""ProposalId"" = @Id",
            new { Id = proposalId, Status = status });
        return affected > 0;
    }
}
