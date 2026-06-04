using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Proposals;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories
{
    public class ProposalRepository : IProposalRepository
    {
        private readonly DapperContext _context;

        public ProposalRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Proposal?> CreateAsync(Proposal proposal)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    INSERT INTO ""Proposals"" (
                        ""ProposerId"", 
                        ""RecipientId"", 
                        ""ProposerUserSkillId"", 
                        ""RecipientUserSkillId"", 
                        ""CreditAmount"",
                        ""Status"", 
                        ""ProposerConfirmed"",
                        ""RecipientConfirmed"",
                        ""CreatedAt"", 
                        ""UpdatedAt""
                    )
                    VALUES (
                        @ProposerId, 
                        @RecipientId, 
                        @ProposerUserSkillId, 
                        @RecipientUserSkillId, 
                        @CreditAmount,
                        @Status,
                        @ProposerConfirmed,
                        @RecipientConfirmed,
                        @CreatedAt, 
                        @UpdatedAt
                    )
                    RETURNING ""ProposalId""";

                var proposalId = await connection.QuerySingleAsync<int>(sql, new
                {
                    proposal.ProposerId,
                    proposal.RecipientId,
                    proposal.ProposerUserSkillId,
                    proposal.RecipientUserSkillId,
                    proposal.CreditAmount,
                    Status = (int)proposal.Status,
                    proposal.ProposerConfirmed,
                    proposal.RecipientConfirmed,
                    proposal.CreatedAt,
                    proposal.UpdatedAt
                }, transaction);

                proposal.ProposalId = proposalId;
                transaction.Commit();
                return proposal;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> CreateCounterofferAsync(ProposalCounteroffer counteroffer)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var insertSql = @"
                    INSERT INTO ""ProposalCounteroffers"" (
                        ""ProposalId"",
                        ""OfferedByUserId"",
                        ""CreditAmount"",
                        ""Message"",
                        ""CreatedAt""
                    )
                    VALUES (
                        @ProposalId,
                        @OfferedByUserId,
                        @CreditAmount,
                        @Message,
                        @CreatedAt
                    )
                    RETURNING ""ProposalCounterofferId""";

                var counterofferId = await connection.QuerySingleAsync<long>(insertSql, new
                {
                    counteroffer.ProposalId,
                    counteroffer.OfferedByUserId,
                    counteroffer.CreditAmount,
                    counteroffer.Message,
                    counteroffer.CreatedAt
                }, transaction);

                counteroffer.ProposalCounterofferId = counterofferId;

                var updateSql = @"
                    UPDATE ""Proposals""
                    SET ""CreditAmount"" = @CreditAmount,
                        ""UpdatedAt"" = @UpdatedAt
                    WHERE ""ProposalId"" = @ProposalId";

                var rowsAffected = await connection.ExecuteAsync(updateSql, new
                {
                    counteroffer.ProposalId,
                    counteroffer.CreditAmount,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<ProposalDto?> GetByIdAsync(int proposalId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT 
                    p.""ProposalId"",
                    p.""ProposerId"",
                    proposer.""UserName"" AS ""ProposerUsername"",
                    proposer.""ProfilePictureURL"" AS ""ProposerProfilePicture"",
                    p.""RecipientId"",
                    recipient.""UserName"" AS ""RecipientUsername"",
                    recipient.""ProfilePictureURL"" AS ""RecipientProfilePicture"",
                    p.""ProposerUserSkillId"",
                    proposerSkill.""SkillName"" AS ""ProposerSkillName"",
                    proposerSkill.""Category"" AS ""ProposerSkillCategory"",
                    proposerUserSkill.""Description"" AS ""ProposerSkillDescription"",
                    p.""RecipientUserSkillId"",
                    recipientSkill.""SkillName"" AS ""RecipientSkillName"",
                    recipientSkill.""Category"" AS ""RecipientSkillCategory"",
                    recipientUserSkill.""Description"" AS ""RecipientSkillDescription"",
                    p.""CreditAmount"",
                    p.""Status"",
                    p.""ProposerConfirmed"",
                    p.""RecipientConfirmed"",
                    p.""CreatedAt"",
                    p.""UpdatedAt""
                FROM ""Proposals"" p
                INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
                INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
                INNER JOIN ""UserSkills"" proposerUserSkill ON p.""ProposerUserSkillId"" = proposerUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" proposerSkill ON proposerUserSkill.""SkillId"" = proposerSkill.""SkillId""
                INNER JOIN ""UserSkills"" recipientUserSkill ON p.""RecipientUserSkillId"" = recipientUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" recipientSkill ON recipientUserSkill.""SkillId"" = recipientSkill.""SkillId""
                WHERE p.""ProposalId"" = @ProposalId";

            var result = await connection.QueryFirstOrDefaultAsync<ProposalQueryResult>(sql, new { ProposalId = proposalId });

            if (result == null) return null;

            var status = (ProposalStatus)result.Status;

            return new ProposalDto
            {
                ProposalId = result.ProposalId,
                ProposerId = result.ProposerId,
                ProposerUsername = result.ProposerUsername,
                ProposerProfilePicture = result.ProposerProfilePicture,
                RecipientId = result.RecipientId,
                RecipientUsername = result.RecipientUsername,
                RecipientProfilePicture = result.RecipientProfilePicture,
                ProposerUserSkillId = result.ProposerUserSkillId,
                ProposerSkillName = result.ProposerSkillName,
                ProposerSkillCategory = result.ProposerSkillCategory,
                ProposerSkillDescription = result.ProposerSkillDescription,
                RecipientUserSkillId = result.RecipientUserSkillId,
                RecipientSkillName = result.RecipientSkillName,
                RecipientSkillCategory = result.RecipientSkillCategory,
                RecipientSkillDescription = result.RecipientSkillDescription,
                CreditAmount = result.CreditAmount,
                Status = status.ToString(),
                ProposerConfirmed = result.ProposerConfirmed,
                RecipientConfirmed = result.RecipientConfirmed,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                Counteroffers = await GetCounteroffersAsync(proposalId)
            };
        }

        public async Task<Proposal?> GetEntityByIdAsync(int proposalId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT 
                    ""ProposalId"",
                    ""ProposerId"",
                    ""RecipientId"",
                    ""ProposerUserSkillId"",
                    ""RecipientUserSkillId"",
                    ""CreditAmount"",
                    ""Status"",
                    ""ProposerConfirmed"",
                    ""RecipientConfirmed"",
                    ""CreatedAt"",
                    ""UpdatedAt""
                FROM ""Proposals""
                WHERE ""ProposalId"" = @ProposalId";

            return await connection.QueryFirstOrDefaultAsync<Proposal>(sql, new { ProposalId = proposalId });
        }

        public async Task<(List<ProposalListDto> Proposals, int TotalCount)> GetUserProposalsAsync(
            string userId, 
            ProposalFilterDto filter)
        {
            using var connection = _context.CreateConnection();

            // Build dynamic WHERE clause
            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            // Direction filter
            if (!string.IsNullOrWhiteSpace(filter.Direction))
            {
                if (filter.Direction.Equals("sent", StringComparison.OrdinalIgnoreCase))
                {
                    whereConditions.Add(@"p.""ProposerId"" = @UserId");
                }
                else if (filter.Direction.Equals("received", StringComparison.OrdinalIgnoreCase))
                {
                    whereConditions.Add(@"p.""RecipientId"" = @UserId");
                }
                else
                {
                    whereConditions.Add(@"(p.""ProposerId"" = @UserId OR p.""RecipientId"" = @UserId)");
                }
            }
            else
            {
                whereConditions.Add(@"(p.""ProposerId"" = @UserId OR p.""RecipientId"" = @UserId)");
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(filter.Status) && 
                Enum.TryParse<ProposalStatus>(filter.Status, true, out var statusEnum))
            {
                whereConditions.Add(@"p.""Status"" = @Status");
                parameters.Add("Status", (int)statusEnum);
            }

            // Search query filter - search in username OR offered skill OR received skill
            if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                var searchPattern = $"%{filter.SearchQuery.ToLower()}%";
                whereConditions.Add(@"(
                    LOWER(proposer.""UserName"") LIKE @SearchPattern OR
                    LOWER(recipient.""UserName"") LIKE @SearchPattern OR
                    LOWER(proposerSkill.""SkillName"") LIKE @SearchPattern OR
                    LOWER(recipientSkill.""SkillName"") LIKE @SearchPattern
                )");
                parameters.Add("SearchPattern", searchPattern);
            }

            // Date range filters
            if (filter.DateFrom.HasValue)
            {
                whereConditions.Add(@"p.""CreatedAt"" >= @DateFrom");
                parameters.Add("DateFrom", filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                whereConditions.Add(@"p.""CreatedAt"" <= @DateTo");
                parameters.Add("DateTo", filter.DateTo.Value);
            }

            var whereClause = string.Join(" AND ", whereConditions);

            // Dynamic ORDER BY clause
            var sortField = filter.SortBy switch
            {
                "CreatedAt" => @"""CreatedAt""",
                "Status" => @"""Status""",
                _ => @"""UpdatedAt"""
            };

            var sortDirection = filter.SortOrder?.Equals("asc", StringComparison.OrdinalIgnoreCase) == true ? "ASC" : "DESC";
            var orderByClause = $"ORDER BY p.{sortField} {sortDirection}";

            // Count query (needs joins for search filter)
            var countSql = $@"
                SELECT COUNT(*)
                FROM ""Proposals"" p
                INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
                INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
                INNER JOIN ""UserSkills"" proposerUserSkill ON p.""ProposerUserSkillId"" = proposerUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" proposerSkill ON proposerUserSkill.""SkillId"" = proposerSkill.""SkillId""
                INNER JOIN ""UserSkills"" recipientUserSkill ON p.""RecipientUserSkillId"" = recipientUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" recipientSkill ON recipientUserSkill.""SkillId"" = recipientSkill.""SkillId""
                WHERE {whereClause}";

            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // Pagination
            var offset = (Math.Max(1, filter.Page) - 1) * Math.Min(50, Math.Max(1, filter.PageSize));
            var limit = Math.Min(50, Math.Max(1, filter.PageSize));
            parameters.Add("Offset", offset);
            parameters.Add("Limit", limit);

            // Data query
            var dataSql = $@"
                SELECT 
                    p.""ProposalId"",
                    p.""ProposerId"",
                    p.""RecipientId"",
                    proposer.""UserName"" AS ""ProposerUsername"",
                    proposer.""ProfilePictureURL"" AS ""ProposerProfilePicture"",
                    recipient.""UserName"" AS ""RecipientUsername"",
                    recipient.""ProfilePictureURL"" AS ""RecipientProfilePicture"",
                    proposerSkill.""SkillName"" AS ""ProposerSkillName"",
                    recipientSkill.""SkillName"" AS ""RecipientSkillName"",
                    p.""CreditAmount"",
                    p.""Status"",
                    p.""ProposerConfirmed"",
                    p.""RecipientConfirmed"",
                    p.""CreatedAt"",
                    p.""UpdatedAt""
                FROM ""Proposals"" p
                INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
                INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
                INNER JOIN ""UserSkills"" proposerUserSkill ON p.""ProposerUserSkillId"" = proposerUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" proposerSkill ON proposerUserSkill.""SkillId"" = proposerSkill.""SkillId""
                INNER JOIN ""UserSkills"" recipientUserSkill ON p.""RecipientUserSkillId"" = recipientUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" recipientSkill ON recipientUserSkill.""SkillId"" = recipientSkill.""SkillId""
                WHERE {whereClause}
                {orderByClause}
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

            var results = await connection.QueryAsync<ProposalListQueryResult>(dataSql, parameters);

            var proposals = results.Select(row =>
            {
                var isProposer = row.ProposerId == userId;
                return new ProposalListDto
                {
                    ProposalId = row.ProposalId,
                    OtherUserId = isProposer ? row.RecipientId : row.ProposerId,
                    OtherUsername = isProposer ? row.RecipientUsername : row.ProposerUsername,
                    OtherProfilePicture = isProposer ? row.RecipientProfilePicture : row.ProposerProfilePicture,
                    OfferingSkillName = isProposer ? row.ProposerSkillName : row.RecipientSkillName,
                    ReceivingSkillName = isProposer ? row.RecipientSkillName : row.ProposerSkillName,
                    CreditAmount = row.CreditAmount,
                    Status = ((ProposalStatus)row.Status).ToString(),
                    ProposerConfirmed = row.ProposerConfirmed,
                    RecipientConfirmed = row.RecipientConfirmed,
                    IsProposer = isProposer,
                    CreatedAt = row.CreatedAt,
                    UpdatedAt = row.UpdatedAt
                };
            }).ToList();

            return (proposals, totalCount);
        }

        public async Task<List<ProposalCounterofferDto>> GetCounteroffersAsync(int proposalId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT
                    pc.""ProposalCounterofferId"",
                    pc.""ProposalId"",
                    pc.""OfferedByUserId"",
                    u.""UserName"" AS ""OfferedByUsername"",
                    pc.""CreditAmount"",
                    pc.""Message"",
                    pc.""CreatedAt""
                FROM ""ProposalCounteroffers"" pc
                INNER JOIN ""AspNetUsers"" u ON pc.""OfferedByUserId"" = u.""Id""
                WHERE pc.""ProposalId"" = @ProposalId
                ORDER BY pc.""CreatedAt"" ASC";

            var rows = await connection.QueryAsync<ProposalCounterofferQueryResult>(sql, new { ProposalId = proposalId });

            return rows.Select(row => new ProposalCounterofferDto
            {
                ProposalCounterofferId = row.ProposalCounterofferId,
                ProposalId = row.ProposalId,
                OfferedByUserId = row.OfferedByUserId,
                OfferedByUsername = row.OfferedByUsername,
                CreditAmount = row.CreditAmount,
                Message = row.Message,
                CreatedAt = row.CreatedAt
            }).ToList();
        }

        public async Task<bool> UpdateStatusAsync(int proposalId, ProposalStatus newStatus)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                UPDATE ""Proposals""
                SET ""Status"" = @Status, ""UpdatedAt"" = @UpdatedAt
                WHERE ""ProposalId"" = @ProposalId";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                ProposalId = proposalId,
                Status = (int)newStatus,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCompletionConfirmationAsync(int proposalId, string userId, bool isProposer)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var updateColumn = isProposer ? "ProposerConfirmed" : "RecipientConfirmed";

                var sql = $@"
                    UPDATE ""Proposals""
                    SET ""{updateColumn}"" = true, ""UpdatedAt"" = @UpdatedAt
                    WHERE ""ProposalId"" = @ProposalId";

                await connection.ExecuteAsync(sql, new
                {
                    ProposalId = proposalId,
                    UpdatedAt = DateTime.UtcNow
                }, transaction);

                // Check if both parties have confirmed
                var checkSql = @"
                    SELECT ""ProposerConfirmed"", ""RecipientConfirmed""
                    FROM ""Proposals""
                    WHERE ""ProposalId"" = @ProposalId";

                var result = await connection.QueryFirstAsync<ConfirmationCheckResult>(checkSql, new { ProposalId = proposalId }, transaction);

                // If both confirmed, update status to Completed
                if (result.ProposerConfirmed && result.RecipientConfirmed)
                {
                    var completeSql = @"
                        UPDATE ""Proposals""
                        SET ""Status"" = @Status, ""UpdatedAt"" = @UpdatedAt
                        WHERE ""ProposalId"" = @ProposalId";

                    await connection.ExecuteAsync(completeSql, new
                    {
                        ProposalId = proposalId,
                        Status = (int)ProposalStatus.Completed,
                        UpdatedAt = DateTime.UtcNow
                    }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> HasActiveProposalAsync(
            string proposerId, 
            string recipientId, 
            int proposerUserSkillId, 
            int recipientUserSkillId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(*)
                FROM ""Proposals""
                WHERE ""ProposerId"" = @ProposerId
                  AND ""RecipientId"" = @RecipientId
                  AND ""ProposerUserSkillId"" = @ProposerUserSkillId
                  AND ""RecipientUserSkillId"" = @RecipientUserSkillId
                  AND ""Status"" IN (@Pending, @Accepted)";

            var count = await connection.ExecuteScalarAsync<int>(sql, new
            {
                ProposerId = proposerId,
                RecipientId = recipientId,
                ProposerUserSkillId = proposerUserSkillId,
                RecipientUserSkillId = recipientUserSkillId,
                Pending = (int)ProposalStatus.Pending,
                Accepted = (int)ProposalStatus.Accepted
            });

            return count > 0;
        }

        public async Task<string?> GetUserSkillOwnerAsync(int userSkillId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT ""UserId""
                FROM ""UserSkills""
                WHERE ""UserSkillId"" = @UserSkillId";

            return await connection.QueryFirstOrDefaultAsync<string>(sql, new { UserSkillId = userSkillId });
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateSkillsForProposalAsync(
            string proposerId,
            int proposerUserSkillId, 
            int recipientUserSkillId)
        {
            using var connection = _context.CreateConnection();

            // Get proposer's skill info
            var proposerSkillSql = @"
                SELECT ""UserId"", ""Type""
                FROM ""UserSkills""
                WHERE ""UserSkillId"" = @UserSkillId";

            var proposerSkill = await connection.QueryFirstOrDefaultAsync<UserSkillValidation>(
                proposerSkillSql, 
                new { UserSkillId = proposerUserSkillId });

            if (proposerSkill == null)
                return (false, "Your selected skill does not exist.");

            if (proposerSkill.UserId != proposerId)
                return (false, "You do not own the selected skill to offer.");

            if ((SkillType)proposerSkill.Type != SkillType.Offer)
                return (false, "You can only offer skills from your 'Offers' list.");

            // Get recipient's skill info
            var recipientSkill = await connection.QueryFirstOrDefaultAsync<UserSkillValidation>(
                proposerSkillSql, 
                new { UserSkillId = recipientUserSkillId });

            if (recipientSkill == null)
                return (false, "The requested skill does not exist.");

            if (recipientSkill.UserId == proposerId)
                return (false, "You cannot request your own skill.");

            if ((SkillType)recipientSkill.Type != SkillType.Offer)
                return (false, "You can only request skills that the other user is offering.");

            return (true, null);
        }
    }
    
    // Internal DTO for skill validation
    internal class UserSkillValidation
    {
        public string UserId { get; set; } = string.Empty;
        public int Type { get; set; }
    }

    // Internal DTO for proposal query result
    internal class ProposalQueryResult
    {
        public int ProposalId { get; set; }
        public string ProposerId { get; set; } = string.Empty;
        public string ProposerUsername { get; set; } = string.Empty;
        public string? ProposerProfilePicture { get; set; }
        public string RecipientId { get; set; } = string.Empty;
        public string RecipientUsername { get; set; } = string.Empty;
        public string? RecipientProfilePicture { get; set; }
        public int ProposerUserSkillId { get; set; }
        public string ProposerSkillName { get; set; } = string.Empty;
        public string ProposerSkillCategory { get; set; } = string.Empty;
        public string? ProposerSkillDescription { get; set; }
        public int RecipientUserSkillId { get; set; }
        public string RecipientSkillName { get; set; } = string.Empty;
        public string RecipientSkillCategory { get; set; } = string.Empty;
        public string? RecipientSkillDescription { get; set; }
        public decimal CreditAmount { get; set; }
        public int Status { get; set; }
        public bool ProposerConfirmed { get; set; }
        public bool RecipientConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Internal DTO for proposal list query (similar to ProposalQueryResult but used in list queries)
    internal class ProposalListQueryResult
    {
        public int ProposalId { get; set; }
        public string ProposerId { get; set; } = string.Empty;
        public string ProposerUsername { get; set; } = string.Empty;
        public string? ProposerProfilePicture { get; set; }
        public string RecipientId { get; set; } = string.Empty;
        public string RecipientUsername { get; set; } = string.Empty;
        public string? RecipientProfilePicture { get; set; }
        public string ProposerSkillName { get; set; } = string.Empty;
        public string RecipientSkillName { get; set; } = string.Empty;
        public decimal CreditAmount { get; set; }
        public int Status { get; set; }
        public bool ProposerConfirmed { get; set; }
        public bool RecipientConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    internal class ProposalCounterofferQueryResult
    {
        public long ProposalCounterofferId { get; set; }
        public int ProposalId { get; set; }
        public string OfferedByUserId { get; set; } = string.Empty;
        public string OfferedByUsername { get; set; } = string.Empty;
        public decimal CreditAmount { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Internal DTO for confirmation check
    internal class ConfirmationCheckResult
    {
        public bool ProposerConfirmed { get; set; }
        public bool RecipientConfirmed { get; set; }
    }
}
