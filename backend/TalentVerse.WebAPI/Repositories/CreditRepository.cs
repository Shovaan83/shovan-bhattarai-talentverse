using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Credits;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class CreditRepository : ICreditRepository
{
    private readonly DapperContext _dapperContext;

    public CreditRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public async Task<decimal> GetBalanceAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"SELECT ""CreditBalance"" FROM ""AspNetUsers"" WHERE ""Id"" = @UserId";
        return await connection.ExecuteScalarAsync<decimal>(sql, new { UserId = userId });
    }

    public async Task<CreditTransaction> AddTransactionAsync(CreditTransaction transaction)
    {
        using var connection = _dapperContext.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        try
        {
            var sql = @"
                INSERT INTO ""CreditTransactions""
                    (""UserId"", ""Type"", ""Amount"", ""TransactionDate"", ""Description"", ""ReferenceId"", ""ReferenceType"", ""BalanceAfter"")
                VALUES
                    (@UserId, @Type, @Amount, @TransactionDate, @Description, @ReferenceId, @ReferenceType, @BalanceAfter)
                RETURNING *";

            var result = await connection.QuerySingleAsync<CreditTransaction>(sql, transaction, transaction: tx);
            tx.Commit();
            return result;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateBalanceAsync(string userId, decimal newBalance)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"UPDATE ""AspNetUsers"" SET ""CreditBalance"" = @Balance WHERE ""Id"" = @UserId";
        var rows = await connection.ExecuteAsync(sql, new { Balance = newBalance, UserId = userId });
        return rows > 0;
    }

    public async Task<TransactionListResponseDto> GetTransactionsAsync(string userId, TransactionFilterDto filter)
    {
        using var connection = _dapperContext.CreateConnection();

        var conditions = new List<string> { @"""UserId"" = @UserId" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (!string.IsNullOrWhiteSpace(filter.Type) && int.TryParse(filter.Type, out var typeInt))
        {
            conditions.Add(@"""Type"" = @Type");
            parameters.Add("Type", typeInt);
        }
        if (filter.From.HasValue)
        {
            conditions.Add(@"""TransactionDate"" >= @From");
            parameters.Add("From", filter.From.Value);
        }
        if (filter.To.HasValue)
        {
            conditions.Add(@"""TransactionDate"" <= @To");
            parameters.Add("To", filter.To.Value);
        }

        var where = string.Join(" AND ", conditions);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Limit", filter.PageSize);
        parameters.Add("Offset", offset);

        var countSql = $@"SELECT COUNT(1) FROM ""CreditTransactions"" WHERE {where}";
        var dataSql = $@"
            SELECT ""TransactionId"", ""UserId"", ""Type"", ""Amount"", ""BalanceAfter"",
                   ""TransactionDate"", ""Description"", ""ReferenceId"", ""ReferenceType""
            FROM ""CreditTransactions""
            WHERE {where}
            ORDER BY ""TransactionDate"" DESC
            LIMIT @Limit OFFSET @Offset";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var rows = await connection.QueryAsync<CreditTransactionRaw>(dataSql, parameters);

        var transactions = rows.Select(r => new CreditTransactionDto
        {
            TransactionId = r.TransactionId,
            UserId = r.UserId,
            Type = (Data.Enums.TransactionType)r.Type,
            TypeLabel = ((Data.Enums.TransactionType)r.Type).ToString(),
            Amount = r.Amount,
            BalanceAfter = r.BalanceAfter,
            TransactionDate = r.TransactionDate,
            Description = r.Description,
            ReferenceId = r.ReferenceId,
            ReferenceType = r.ReferenceType
        });

        return new TransactionListResponseDto
        {
            Transactions = transactions,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
        };
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 50)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT
                ROW_NUMBER() OVER (ORDER BY u.""CreditBalance"" DESC) AS ""Rank"",
                u.""Id""                    AS ""UserId"",
                u.""UserName""              AS ""Username"",
                u.""ProfilePictureURL""     AS ""ProfilePictureUrl"",
                u.""CreditBalance"",
                COALESCE(sw.""CompletedSwaps"", 0) AS ""CompletedSwaps"",
                COALESCE(b.""BadgeCount"", 0)      AS ""BadgeCount""
            FROM ""AspNetUsers"" u
            LEFT JOIN (
                SELECT p.""ProposerId"" AS ""UserId"", COUNT(*) AS ""CompletedSwaps""
                FROM ""Proposals"" p WHERE p.""Status"" = 3
                GROUP BY p.""ProposerId""
            ) sw ON sw.""UserId"" = u.""Id""
            LEFT JOIN (
                SELECT ub.""UserId"", COUNT(*) AS ""BadgeCount""
                FROM ""UserBadges"" ub GROUP BY ub.""UserId""
            ) b ON b.""UserId"" = u.""Id""
            ORDER BY u.""CreditBalance"" DESC
            LIMIT @Limit";

        return await connection.QueryAsync<LeaderboardEntryDto>(sql, new { Limit = limit });
    }

    public async Task<int?> GetUserRankAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT rank FROM (
                SELECT ""Id"", ROW_NUMBER() OVER (ORDER BY ""CreditBalance"" DESC) AS rank
                FROM ""AspNetUsers""
            ) ranked
            WHERE ""Id"" = @UserId";

        var result = await connection.ExecuteScalarAsync<int?>(sql, new { UserId = userId });
        return result;
    }

    public async Task<int> GetCompletedSwapCountAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT COUNT(*)
            FROM ""Proposals""
            WHERE (""ProposerId"" = @UserId OR ""RecipientId"" = @UserId)
              AND ""Status"" = 3";
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<int> GetReviewCountAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"SELECT COUNT(*) FROM ""Reviews"" WHERE ""ReviewerId"" = @UserId";
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<int> GetSkillCountAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"SELECT COUNT(*) FROM ""UserSkills"" WHERE ""UserId"" = @UserId";
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
    }

    public async Task<double> GetAverageRatingAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"SELECT COALESCE(AVG(CAST(""Rating"" AS FLOAT)), 0) FROM ""Reviews"" WHERE ""RevieweeId"" = @UserId";
        return await connection.ExecuteScalarAsync<double>(sql, new { UserId = userId });
    }

    public async Task<bool> HasTransactionByReferenceAsync(string referenceType, string referenceId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"SELECT COUNT(1) FROM ""CreditTransactions""
                    WHERE ""ReferenceType"" = @ReferenceType AND ""Description"" LIKE '%' || @ReferenceId || '%'";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { ReferenceType = referenceType, ReferenceId = referenceId });
        return count > 0;
    }

    // Internal helper — raw row shape matching DB columns (Type stored as int)
    private class CreditTransactionRaw
    {
        public long TransactionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public long? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }
}
