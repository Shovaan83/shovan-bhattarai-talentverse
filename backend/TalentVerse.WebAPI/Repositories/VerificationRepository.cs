using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Verification;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class VerificationRepository : IVerificationRepository
{
    private readonly DapperContext _dapperContext;

    public VerificationRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public async Task<VerificationRequest?> CreateRequestAsync(VerificationRequest request)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            INSERT INTO ""VerificationRequests""
                (""UserId"", ""DocumentUrl"", ""DocumentPublicId"", ""Status"", ""SubmittedAt"")
            VALUES
                (@UserId, @DocumentUrl, @DocumentPublicId, @Status, @SubmittedAt)
            RETURNING
                ""VerificationRequestId"", ""UserId"", ""DocumentUrl"", ""DocumentPublicId"",
                ""Status"", ""SubmittedAt"", ""ReviewedAt"", ""ReviewedByUserId"",
                ""AdminNotes"", ""RejectionReason""";

        return await connection.QuerySingleOrDefaultAsync<VerificationRequest>(sql, new
        {
            request.UserId,
            request.DocumentUrl,
            request.DocumentPublicId,
            Status = (int)request.Status,
            request.SubmittedAt
        });
    }

    public async Task<VerificationRequest?> GetByIdAsync(long id)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT
                ""VerificationRequestId"", ""UserId"", ""DocumentUrl"", ""DocumentPublicId"",
                ""Status"", ""SubmittedAt"", ""ReviewedAt"", ""ReviewedByUserId"",
                ""AdminNotes"", ""RejectionReason""
            FROM ""VerificationRequests""
            WHERE ""VerificationRequestId"" = @Id";

        return await connection.QuerySingleOrDefaultAsync<VerificationRequest>(sql, new { Id = id });
    }

    public async Task<VerificationRequest?> GetLatestByUserIdAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT
                ""VerificationRequestId"", ""UserId"", ""DocumentUrl"", ""DocumentPublicId"",
                ""Status"", ""SubmittedAt"", ""ReviewedAt"", ""ReviewedByUserId"",
                ""AdminNotes"", ""RejectionReason""
            FROM ""VerificationRequests""
            WHERE ""UserId"" = @UserId
            ORDER BY ""SubmittedAt"" DESC
            LIMIT 1";

        return await connection.QuerySingleOrDefaultAsync<VerificationRequest>(sql, new { UserId = userId });
    }

    public async Task<(List<VerificationRequestDto> Requests, int TotalCount)> GetPendingRequestsAsync(int page, int pageSize)
    {
        using var connection = _dapperContext.CreateConnection();
        var offset = (page - 1) * pageSize;

        // Get total count
        var countSql = @"
            SELECT COUNT(*)
            FROM ""VerificationRequests""
            WHERE ""Status"" = @Status";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Status = (int)VerificationStatus.Pending });

        // Get paginated results with user details
        var sql = @"
            SELECT
                vr.""VerificationRequestId"" AS Id,
                vr.""UserId"",
                u.""UserName"",
                u.""Email"" AS UserEmail,
                u.""ProfilePictureURL"" AS UserProfilePictureUrl,
                vr.""DocumentUrl"",
                vr.""DocumentPublicId"",
                CASE vr.""Status""
                    WHEN 0 THEN 'None'
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'Approved'
                    WHEN 3 THEN 'Rejected'
                END AS Status,
                vr.""SubmittedAt"",
                vr.""ReviewedAt"",
                reviewer.""UserName"" AS ReviewedByUserName,
                vr.""AdminNotes"",
                vr.""RejectionReason""
            FROM ""VerificationRequests"" vr
            INNER JOIN ""AspNetUsers"" u ON vr.""UserId"" = u.""Id""
            LEFT JOIN ""AspNetUsers"" reviewer ON vr.""ReviewedByUserId"" = reviewer.""Id""
            WHERE vr.""Status"" = @Status
            ORDER BY vr.""SubmittedAt"" ASC
            OFFSET @Offset LIMIT @PageSize";

        var requests = (await connection.QueryAsync<VerificationRequestDto>(sql, new
        {
            Status = (int)VerificationStatus.Pending,
            Offset = offset,
            PageSize = pageSize
        })).ToList();

        return (requests, totalCount);
    }

    public async Task<bool> UpdateRequestAsync(VerificationRequest request)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            UPDATE ""VerificationRequests""
            SET
                ""Status"" = @Status,
                ""ReviewedAt"" = @ReviewedAt,
                ""ReviewedByUserId"" = @ReviewedByUserId,
                ""AdminNotes"" = @AdminNotes,
                ""RejectionReason"" = @RejectionReason
            WHERE ""VerificationRequestId"" = @VerificationRequestId";

        var rows = await connection.ExecuteAsync(sql, new
        {
            request.VerificationRequestId,
            Status = (int)request.Status,
            request.ReviewedAt,
            request.ReviewedByUserId,
            request.AdminNotes,
            request.RejectionReason
        });

        return rows > 0;
    }

    public async Task<VerificationStatusDto> GetUserVerificationStatusAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();

        // First check if user is verified
        var userSql = @"
            SELECT ""IsIdentityVerified"", ""VerifiedAt""
            FROM ""AspNetUsers""
            WHERE ""Id"" = @UserId";

        var user = await connection.QuerySingleOrDefaultAsync<(bool IsVerified, DateTime? VerifiedAt)>(userSql, new { UserId = userId });

        if (user.IsVerified)
        {
            return new VerificationStatusDto
            {
                Status = "Approved",
                IsVerified = true,
                VerifiedAt = user.VerifiedAt
            };
        }

        // Check latest verification request
        var requestSql = @"
            SELECT
                ""Status"", ""SubmittedAt"", ""ReviewedAt"", ""RejectionReason""
            FROM ""VerificationRequests""
            WHERE ""UserId"" = @UserId
            ORDER BY ""SubmittedAt"" DESC
            LIMIT 1";

        var request = await connection.QuerySingleOrDefaultAsync<(int Status, DateTime SubmittedAt, DateTime? ReviewedAt, string? RejectionReason)>(
            requestSql, new { UserId = userId });

        if (request.Status == 0 && request.SubmittedAt == default)
        {
            return new VerificationStatusDto
            {
                Status = "None",
                IsVerified = false
            };
        }

        var statusString = request.Status switch
        {
            1 => "Pending",
            2 => "Approved",
            3 => "Rejected",
            _ => "None"
        };

        return new VerificationStatusDto
        {
            Status = statusString,
            IsVerified = false,
            SubmittedAt = request.SubmittedAt,
            ReviewedAt = request.ReviewedAt,
            RejectionReason = request.RejectionReason
        };
    }

    public async Task<bool> HasPendingRequestAsync(string userId)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            SELECT COUNT(1)
            FROM ""VerificationRequests""
            WHERE ""UserId"" = @UserId AND ""Status"" = @Status";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, Status = (int)VerificationStatus.Pending });
        return count > 0;
    }

    public async Task<bool> UpdateUserVerificationStatusAsync(string userId, bool isVerified, DateTime? verifiedAt)
    {
        using var connection = _dapperContext.CreateConnection();
        var sql = @"
            UPDATE ""AspNetUsers""
            SET ""IsIdentityVerified"" = @IsVerified, ""VerifiedAt"" = @VerifiedAt
            WHERE ""Id"" = @UserId";

        var rows = await connection.ExecuteAsync(sql, new { UserId = userId, IsVerified = isVerified, VerifiedAt = verifiedAt });
        return rows > 0;
    }
}
