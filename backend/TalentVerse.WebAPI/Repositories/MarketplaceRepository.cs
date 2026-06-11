using Dapper;
using Microsoft.Extensions.Logging;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.DTO.Marketplace;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class MarketplaceRepository : IMarketplaceRepository
{
    private readonly DapperContext _dapperContext;
    private readonly ILogger<MarketplaceRepository> _logger;

    public MarketplaceRepository(DapperContext dapperContext, ILogger<MarketplaceRepository> logger)
    {
        _dapperContext = dapperContext;
        _logger = logger;
    }

    public async Task<(List<PublicUserDto> Users, int TotalCount)> SearchUsersAsync(UserSearchDto searchDto, string? excludeUserId = null)
    {
        using var connection = _dapperContext.CreateConnection();

        var offset = (searchDto.Page - 1) * searchDto.PageSize;

        // Build dynamic WHERE clause
        var whereConditions = new List<string> { "1=1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(excludeUserId))
        {
            whereConditions.Add("u.\"Id\" != @ExcludeUserId");
            parameters.Add("ExcludeUserId", excludeUserId);
        }

        // Search by username (no DisplayName column exists)
        if (!string.IsNullOrWhiteSpace(searchDto.Query))
        {
            whereConditions.Add("LOWER(u.\"UserName\") LIKE @Query");
            parameters.Add("Query", $"%{searchDto.Query.ToLower()}%");
        }

        // Filter by skill name - JOIN UserSkills with Skills table
        if (!string.IsNullOrWhiteSpace(searchDto.SkillName))
        {
            var skillTypeFilter = searchDto.SkillType == "Wanted" ? 1 : 0; // 0 = Offer, 1 = Want
            whereConditions.Add(@"EXISTS (
                SELECT 1 FROM ""UserSkills"" us 
                INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
                WHERE us.""UserId"" = u.""Id"" 
                AND LOWER(s.""SkillName"") LIKE @SkillName
                AND us.""Type"" = @SkillTypeFilter
            )");
            parameters.Add("SkillName", $"%{searchDto.SkillName.ToLower()}%");
            parameters.Add("SkillTypeFilter", skillTypeFilter);
        }

        // Filter by category - JOIN UserSkills with Skills table
        if (!string.IsNullOrWhiteSpace(searchDto.Category))
        {
            whereConditions.Add(@"EXISTS (
                SELECT 1 FROM ""UserSkills"" us 
                INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
                WHERE us.""UserId"" = u.""Id"" 
                AND LOWER(s.""Category"") = LOWER(@Category)
            )");
            parameters.Add("Category", searchDto.Category);
        }

        if (searchDto.MinProficiency.HasValue)
        {
            whereConditions.Add(@"EXISTS (
                SELECT 1 FROM ""UserSkills"" us
                WHERE us.""UserId"" = u.""Id""
                AND us.""ProficiencyLevel"" >= @MinProficiency
            )");
            parameters.Add("MinProficiency", searchDto.MinProficiency.Value);
        }

        if (searchDto.MaxProficiency.HasValue)
        {
            whereConditions.Add(@"EXISTS (
                SELECT 1 FROM ""UserSkills"" us
                WHERE us.""UserId"" = u.""Id""
                AND us.""ProficiencyLevel"" <= @MaxProficiency
            )");
            parameters.Add("MaxProficiency", searchDto.MaxProficiency.Value);
        }

        var whereClause = string.Join(" AND ", whereConditions);

        // Count query
        var countSql = $@"
            SELECT COUNT(DISTINCT u.""Id"")
            FROM ""AspNetUsers"" u
            WHERE {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Main query to get user IDs with pagination
        // PostgreSQL requires ORDER BY columns to appear in SELECT list when using DISTINCT
        var userIdsSql = $@"
            SELECT u.""Id"" FROM (
                SELECT DISTINCT u.""Id"", u.""CreatedAt""
                FROM ""AspNetUsers"" u
                WHERE {whereClause}
            ) u
            ORDER BY u.""CreatedAt"" DESC
            OFFSET @Offset LIMIT @PageSize";

        parameters.Add("Offset", offset);
        parameters.Add("PageSize", searchDto.PageSize);

        var userIds = (await connection.QueryAsync<string>(userIdsSql, parameters)).ToList();

        if (!userIds.Any())
        {
            return (new List<PublicUserDto>(), totalCount);
        }

        // Fetch full user data for the paginated results
        var users = await GetUsersWithSkillsAsync(connection, userIds);

        return (users, totalCount);
    }

    public async Task<PublicUserDto?> GetUserProfileAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();

        var users = await GetUsersWithSkillsAsync(connection, new List<string> { userId });
        return users.FirstOrDefault();
    }

    public async Task<List<PublicUserDto>> GetFeaturedUsersAsync(string? excludeUserId = null, int limit = 12)
    {
        using var connection = _dapperContext.CreateConnection();

        // Get users with the most skills and/or completed swaps
        var sql = @"
            SELECT u.""Id""
            FROM ""AspNetUsers"" u
            LEFT JOIN ""UserSkills"" us ON u.""Id"" = us.""UserId""
            WHERE (@ExcludeUserId IS NULL OR u.""Id"" != @ExcludeUserId)
            GROUP BY u.""Id""
            HAVING COUNT(us.""UserSkillId"") > 0
            ORDER BY COUNT(us.""UserSkillId"") DESC, u.""CreatedAt"" DESC
            LIMIT @Limit";

        var userIds = (await connection.QueryAsync<string>(sql, new { ExcludeUserId = excludeUserId, Limit = limit })).ToList();

        if (!userIds.Any())
        {
            return new List<PublicUserDto>();
        }

        return await GetUsersWithSkillsAsync(connection, userIds);
    }

    public async Task<List<SkillBrowseDto>> GetPopularSkillsAsync(string? skillType = null, int limit = 20)
    {
        using var connection = _dapperContext.CreateConnection();

        var skillTypeFilter = skillType switch
        {
            "Offered" => "AND us.\"Type\" = 0",
            "Wanted" => "AND us.\"Type\" = 1",
            _ => ""
        };

        // JOIN UserSkills with Skills to get skill names
        var sql = $@"
            SELECT 
                s.""SkillName"" AS SkillName,
                COUNT(DISTINCT us.""UserId"") AS UserCount,
                AVG(us.""ProficiencyLevel"")::float AS AverageProficiency
            FROM ""UserSkills"" us
            INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
            WHERE 1=1 {skillTypeFilter}
            GROUP BY s.""SkillName""
            ORDER BY UserCount DESC
            LIMIT @Limit";

        var skills = await connection.QueryAsync<SkillBrowseDto>(sql, new { Limit = limit });
        return skills.ToList();
    }

    public async Task<int> GetCompletedSwapsCountAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();

        // Status = 3 corresponds to ProposalStatus.Completed enum value
        var sql = @"
            SELECT COUNT(*)
            FROM ""Proposals"" p
            WHERE (p.""ProposerId"" = @UserId OR p.""RecipientId"" = @UserId)
            AND p.""Status"" = 3";

        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        using var connection = _dapperContext.CreateConnection();

        var sql = @"
            SELECT DISTINCT ""Category""
            FROM ""Skills""
            WHERE ""Category"" IS NOT NULL 
            AND ""IsActive"" = true
            ORDER BY ""Category""";

        var categories = await connection.QueryAsync<string>(sql);
        return categories.ToList();
    }

    private async Task<List<PublicUserDto>> GetUsersWithSkillsAsync(System.Data.IDbConnection connection, List<string> userIds)
    {
        if (!userIds.Any())
        {
            return new List<PublicUserDto>();
        }

        // Use IN clause instead of ANY() for better Dapper compatibility
        // Fetch users - no DisplayName column exists, use UserName only
        // ProfilePictureURL uses capital URL in entity
        var usersSql = @"
            SELECT 
                u.""Id"",
                u.""UserName"",
                u.""UserName"" AS DisplayName,
                u.""Bio"",
                u.""ProfilePictureURL"" AS ProfilePictureUrl,
                u.""CoverPhotoUrl"",
                u.""CreatedAt"" AS JoinedAt
            FROM ""AspNetUsers"" u
            WHERE u.""Id"" = ANY(@UserIds)";

        var users = (await connection.QueryAsync<PublicUserDto>(usersSql, new { UserIds = userIds })).ToList();

        _logger.LogInformation("GetUsersWithSkillsAsync: Found {UserCount} users for IDs: {UserIds}", users.Count, string.Join(", ", userIds));

        if (!users.Any())
        {
            return users;
        }

        // Fetch skills for all users - JOIN with Skills table to get SkillName
        // UserSkillId is the primary key, Type is the enum column (0=Offer, 1=Want)
        var skillsSql = @"
            SELECT 
                us.""UserSkillId"" AS Id,
                us.""UserId"",
                s.""SkillName"",
                us.""ProficiencyLevel"",
                us.""Description"",
                CASE WHEN us.""Type"" = 0 THEN 'Offered' ELSE 'Wanted' END AS SkillType
            FROM ""UserSkills"" us
            INNER JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
            WHERE us.""UserId"" = ANY(@UserIds)";

        var skills = (await connection.QueryAsync<SkillQueryResult>(skillsSql, new { UserIds = userIds })).ToList();
        
        _logger.LogInformation("GetUsersWithSkillsAsync: Found {SkillCount} skills for users", skills.Count);

        // Fetch completed swaps count for all users
        // Status = 3 corresponds to ProposalStatus.Completed enum value
        var swapsSql = @"
            SELECT 
                CASE 
                    WHEN p.""ProposerId"" = ANY(@UserIds) THEN p.""ProposerId""
                    ELSE p.""RecipientId""
                END AS UserId,
                COUNT(*) AS Count
            FROM ""Proposals"" p
            WHERE (p.""ProposerId"" = ANY(@UserIds) OR p.""RecipientId"" = ANY(@UserIds))
            AND p.""Status"" = 3
            GROUP BY CASE 
                WHEN p.""ProposerId"" = ANY(@UserIds) THEN p.""ProposerId""
                ELSE p.""RecipientId""
            END";

        var swapCounts = (await connection.QueryAsync<SwapCountResult>(swapsSql, new { UserIds = userIds }))
            .ToDictionary(x => x.UserId, x => x.Count);

        // Fetch average ratings for all users
        var ratingsSql = @"
            SELECT 
                r.""RevieweeId"" AS UserId,
                AVG(r.""Rating"")::float AS AverageRating
            FROM ""Reviews"" r
            WHERE r.""RevieweeId"" = ANY(@UserIds)
            GROUP BY r.""RevieweeId""";

        var averageRatings = (await connection.QueryAsync<RatingResult>(ratingsSql, new { UserIds = userIds }))
            .ToDictionary(x => x.UserId, x => x.AverageRating);

        // Map skills, swaps, and ratings to users
        foreach (var user in users)
        {
            var userSkills = skills.Where(s => s.UserId == user.Id).ToList();
            
            _logger.LogInformation("User {UserId} has {SkillCount} skills", user.Id, userSkills.Count);
            
            user.OfferedSkills = userSkills
                .Where(s => s.SkillType == "Offered")
                .Select(s => new PublicSkillDto
                {
                    Id = s.Id,
                    SkillName = s.SkillName,
                    ProficiencyLevel = s.ProficiencyLevel,
                    Description = s.Description,
                    SkillType = "Offered"
                })
                .ToList();

            user.WantedSkills = userSkills
                .Where(s => s.SkillType == "Wanted")
                .Select(s => new PublicSkillDto
                {
                    Id = s.Id,
                    SkillName = s.SkillName,
                    ProficiencyLevel = s.ProficiencyLevel,
                    Description = s.Description,
                    SkillType = "Wanted"
                })
                .ToList();

            user.CompletedSwaps = swapCounts.GetValueOrDefault(user.Id, 0);
            user.AverageRating = averageRatings.ContainsKey(user.Id) ? averageRatings[user.Id] : null;
        }

        // Maintain original order
        return userIds.Select(id => users.First(u => u.Id == id)).ToList();
    }
}
