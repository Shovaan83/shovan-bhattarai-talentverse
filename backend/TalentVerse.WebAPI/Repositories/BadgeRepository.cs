using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Badges;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class BadgeRepository : IBadgeRepository
{
    private readonly DapperContext _dapperContext;

    public BadgeRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public async Task<IEnumerable<BadgeDto>> GetAllBadgesWithUserStatusAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT
                b.""BadgeId"",
                b.""Name"",
                b.""Description"",
                b.""IconKey"",
                b.""Tier"",
                b.""Category"",
                b.""CreditReward"",
                CASE WHEN ub.""UserBadgeId"" IS NOT NULL THEN TRUE ELSE FALSE END AS ""IsEarned"",
                ub.""EarnedAt""
            FROM ""Badges"" b
            LEFT JOIN ""UserBadges"" ub ON b.""BadgeId"" = ub.""BadgeId"" AND ub.""UserId"" = @UserId
            ORDER BY ""IsEarned"" DESC, b.""Tier"" DESC, b.""BadgeId""";

        return await connection.QueryAsync<BadgeDto>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<BadgeDto>> GetUserBadgesAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT
                b.""BadgeId"",
                b.""Name"",
                b.""Description"",
                b.""IconKey"",
                b.""Tier"",
                b.""Category"",
                b.""CreditReward"",
                TRUE AS ""IsEarned"",
                ub.""EarnedAt""
            FROM ""Badges"" b
            INNER JOIN ""UserBadges"" ub ON b.""BadgeId"" = ub.""BadgeId""
            WHERE ub.""UserId"" = @UserId
            ORDER BY ub.""EarnedAt"" DESC";

        return await connection.QueryAsync<BadgeDto>(sql, new { UserId = userId });
    }

    public async Task<bool> UserHasBadgeAsync(string userId, int badgeId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT COUNT(1)
            FROM ""UserBadges""
            WHERE ""UserId"" = @UserId AND ""BadgeId"" = @BadgeId";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, BadgeId = badgeId });
        return count > 0;
    }

    public async Task<bool> AwardBadgeAsync(string userId, int badgeId)
    {
        using var connection = _dapperContext.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        try
        {
            var sql = @"
                INSERT INTO ""UserBadges"" (""UserId"", ""BadgeId"", ""EarnedAt"")
                VALUES (@UserId, @BadgeId, @EarnedAt)
                ON CONFLICT (""UserId"", ""BadgeId"") DO NOTHING";

            var rows = await connection.ExecuteAsync(sql,
                new { UserId = userId, BadgeId = badgeId, EarnedAt = DateTime.UtcNow },
                transaction: tx);

            tx.Commit();
            return rows > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<int?> GetBadgeIdByNameAsync(string badgeName)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"SELECT ""BadgeId"" FROM ""Badges"" WHERE ""Name"" = @Name LIMIT 1";
        var result = await connection.ExecuteScalarAsync<int?>(sql, new { Name = badgeName });
        return result;
    }
}
