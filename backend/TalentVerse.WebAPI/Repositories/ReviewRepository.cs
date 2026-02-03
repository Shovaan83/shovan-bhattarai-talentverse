using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Reviews;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly DapperContext _dapperContext;

    public ReviewRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public async Task<Review?> AddReviewAsync(Review review)
    {
        using var connection = _dapperContext.CreateConnection();
        connection.Open();
        
        using var transaction = connection.BeginTransaction();
        
        try
        {
            var sql = @"
                INSERT INTO ""Reviews"" (""ProposalId"", ""ReviewerId"", ""RevieweeId"", ""Rating"", ""Comment"", ""CreatedAt"")
                VALUES (@ProposalId, @ReviewerId, @RevieweeId, @Rating, @Comment, @CreatedAt)
                RETURNING ""ReviewId"", ""ProposalId"", ""ReviewerId"", ""RevieweeId"", ""Rating"", ""Comment"", ""CreatedAt""";
            
            var result = await connection.QuerySingleAsync<Review>(sql, review, transaction: transaction);
            
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        
        var sql = @"
            SELECT 
                r.""ReviewId"",
                r.""ProposalId"",
                r.""ReviewerId"",
                reviewer.""UserName"" AS ""ReviewerUsername"",
                reviewer.""ProfilePictureURL"" AS ""ReviewerProfilePictureUrl"",
                r.""RevieweeId"",
                reviewee.""UserName"" AS ""RevieweeUsername"",
                r.""Rating"",
                r.""Comment"",
                r.""CreatedAt""
            FROM ""Reviews"" r
            INNER JOIN ""AspNetUsers"" reviewer ON r.""ReviewerId"" = reviewer.""Id""
            INNER JOIN ""AspNetUsers"" reviewee ON r.""RevieweeId"" = reviewee.""Id""
            WHERE r.""RevieweeId"" = @UserId
            ORDER BY r.""CreatedAt"" DESC";
        
        return await connection.QueryAsync<ReviewDto>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsForProposalAsync(int proposalId)
    {
        using var connection = _dapperContext.CreateConnection();
        
        var sql = @"
            SELECT 
                r.""ReviewId"",
                r.""ProposalId"",
                r.""ReviewerId"",
                reviewer.""UserName"" AS ""ReviewerUsername"",
                reviewer.""ProfilePictureURL"" AS ""ReviewerProfilePictureUrl"",
                r.""RevieweeId"",
                reviewee.""UserName"" AS ""RevieweeUsername"",
                r.""Rating"",
                r.""Comment"",
                r.""CreatedAt""
            FROM ""Reviews"" r
            INNER JOIN ""AspNetUsers"" reviewer ON r.""ReviewerId"" = reviewer.""Id""
            INNER JOIN ""AspNetUsers"" reviewee ON r.""RevieweeId"" = reviewee.""Id""
            WHERE r.""ProposalId"" = @ProposalId
            ORDER BY r.""CreatedAt"" DESC";
        
        return await connection.QueryAsync<ReviewDto>(sql, new { ProposalId = proposalId });
    }

    public async Task<bool> HasUserReviewedProposalAsync(string userId, int proposalId)
    {
        using var connection = _dapperContext.CreateConnection();
        
        var sql = @"
            SELECT COUNT(1)
            FROM ""Reviews""
            WHERE ""ReviewerId"" = @UserId AND ""ProposalId"" = @ProposalId";
        
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, ProposalId = proposalId });
        return count > 0;
    }

    public async Task<UserReputationDto?> GetUserReputationAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        
        var sql = @"
            SELECT 
                @UserId AS ""UserId"",
                COALESCE(AVG(r.""Rating""), 0) AS ""AverageRating"",
                COUNT(r.""ReviewId"") AS ""TotalReviews"",
                (
                    SELECT COUNT(DISTINCT p.""ProposalId"")
                    FROM ""Proposals"" p
                    WHERE (p.""ProposerId"" = @UserId OR p.""RecipientId"" = @UserId)
                    AND p.""Status"" = 3
                ) AS ""CompletedSwaps""
            FROM ""Reviews"" r
            WHERE r.""RevieweeId"" = @UserId";
        
        return await connection.QuerySingleOrDefaultAsync<UserReputationDto>(sql, new { UserId = userId });
    }
}
