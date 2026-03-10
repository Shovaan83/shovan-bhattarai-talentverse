using Dapper;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Messages;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DapperContext _context;

        public MessageRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<MessageDto?> CreateAsync(Message message, string currentUserId)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var insertSql = @"
                    INSERT INTO ""Messages"" (""ProposalId"", ""SenderId"", ""MessageContent"", ""SentAt"", ""IsRead"")
                    VALUES (@ProposalId, @SenderId, @MessageContent, @SentAt, @IsRead)
                    RETURNING ""MessageId""";

                var messageId = await connection.QuerySingleAsync<int>(insertSql, new
                {
                    message.ProposalId,
                    message.SenderId,
                    message.MessageContent,
                    message.SentAt,
                    message.IsRead
                }, transaction);

                transaction.Commit();

                // Fetch the created message with sender info
                var fetchSql = @"
                    SELECT
                        m.""MessageId"",
                        m.""ProposalId"",
                        m.""SenderId"",
                        u.""UserName"" AS ""SenderUsername"",
                        u.""ProfilePictureURL"" AS ""SenderProfilePicture"",
                        m.""MessageContent"",
                        m.""SentAt"",
                        m.""IsRead"",
                        CASE WHEN m.""SenderId"" = @CurrentUserId THEN TRUE ELSE FALSE END AS ""IsOwnMessage""
                    FROM ""Messages"" m
                    INNER JOIN ""AspNetUsers"" u ON m.""SenderId"" = u.""Id""
                    WHERE m.""MessageId"" = @MessageId";

                return await connection.QueryFirstOrDefaultAsync<MessageDto>(fetchSql, new
                {
                    MessageId = messageId,
                    CurrentUserId = currentUserId
                });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<(List<MessageDto> Messages, int TotalCount)> GetMessagesByProposalAsync(
            int proposalId, string currentUserId, int page, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var countSql = @"SELECT COUNT(*) FROM ""Messages"" WHERE ""ProposalId"" = @ProposalId";
            var totalCount = await connection.QuerySingleAsync<int>(countSql, new { ProposalId = proposalId });

            var sql = @"
                SELECT
                    m.""MessageId"",
                    m.""ProposalId"",
                    m.""SenderId"",
                    u.""UserName"" AS ""SenderUsername"",
                    u.""ProfilePictureURL"" AS ""SenderProfilePicture"",
                    m.""MessageContent"",
                    m.""SentAt"",
                    m.""IsRead"",
                    CASE WHEN m.""SenderId"" = @CurrentUserId THEN TRUE ELSE FALSE END AS ""IsOwnMessage""
                FROM ""Messages"" m
                INNER JOIN ""AspNetUsers"" u ON m.""SenderId"" = u.""Id""
                WHERE m.""ProposalId"" = @ProposalId
                ORDER BY m.""SentAt"" ASC
                LIMIT @PageSize OFFSET @Offset";

            var messages = (await connection.QueryAsync<MessageDto>(sql, new
            {
                ProposalId = proposalId,
                CurrentUserId = currentUserId,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            })).ToList();

            return (messages, totalCount);
        }

        public async Task<List<ConversationDto>> GetUserConversationsAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            // Get all Accepted/Completed proposals for the user,
            // with last message info and unread counts
            var sql = @"
                SELECT
                    p.""ProposalId"",
                    CASE
                        WHEN p.""ProposerId"" = @UserId THEN p.""RecipientId""
                        ELSE p.""ProposerId""
                    END AS ""OtherUserId"",
                    CASE
                        WHEN p.""ProposerId"" = @UserId THEN recipient.""UserName""
                        ELSE proposer.""UserName""
                    END AS ""OtherUsername"",
                    CASE
                        WHEN p.""ProposerId"" = @UserId THEN recipient.""ProfilePictureURL""
                        ELSE proposer.""ProfilePictureURL""
                    END AS ""OtherUserProfilePicture"",
                    CASE
                        WHEN p.""ProposerId"" = @UserId THEN proposerSkill.""SkillName""
                        ELSE recipientSkill.""SkillName""
                    END AS ""OfferingSkillName"",
                    CASE
                        WHEN p.""ProposerId"" = @UserId THEN recipientSkill.""SkillName""
                        ELSE proposerSkill.""SkillName""
                    END AS ""ReceivingSkillName"",
                    p.""Status"" AS ""ProposalStatus"",
                    last_msg.""MessageContent"" AS ""LastMessage"",
                    last_msg.""SentAt"" AS ""LastMessageAt"",
                    COALESCE(unread.""UnreadCount"", 0) AS ""UnreadCount""
                FROM ""Proposals"" p
                INNER JOIN ""AspNetUsers"" proposer ON p.""ProposerId"" = proposer.""Id""
                INNER JOIN ""AspNetUsers"" recipient ON p.""RecipientId"" = recipient.""Id""
                INNER JOIN ""UserSkills"" proposerUserSkill ON p.""ProposerUserSkillId"" = proposerUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" proposerSkill ON proposerUserSkill.""SkillId"" = proposerSkill.""SkillId""
                INNER JOIN ""UserSkills"" recipientUserSkill ON p.""RecipientUserSkillId"" = recipientUserSkill.""UserSkillId""
                INNER JOIN ""Skills"" recipientSkill ON recipientUserSkill.""SkillId"" = recipientSkill.""SkillId""
                LEFT JOIN LATERAL (
                    SELECT ""MessageContent"", ""SentAt""
                    FROM ""Messages""
                    WHERE ""ProposalId"" = p.""ProposalId""
                    ORDER BY ""SentAt"" DESC
                    LIMIT 1
                ) last_msg ON TRUE
                LEFT JOIN (
                    SELECT ""ProposalId"", COUNT(*) AS ""UnreadCount""
                    FROM ""Messages""
                    WHERE ""SenderId"" != @UserId AND ""IsRead"" = FALSE
                    GROUP BY ""ProposalId""
                ) unread ON unread.""ProposalId"" = p.""ProposalId""
                WHERE (p.""ProposerId"" = @UserId OR p.""RecipientId"" = @UserId)
                  AND p.""Status"" IN (1, 3)
                ORDER BY COALESCE(last_msg.""SentAt"", p.""UpdatedAt"") DESC";

            var result = await connection.QueryAsync<ConversationQueryResult>(sql, new { UserId = userId });

            return result.Select(r => new ConversationDto
            {
                ProposalId = r.ProposalId,
                OtherUserId = r.OtherUserId,
                OtherUsername = r.OtherUsername,
                OtherUserProfilePicture = r.OtherUserProfilePicture,
                OfferingSkillName = r.OfferingSkillName,
                ReceivingSkillName = r.ReceivingSkillName,
                ProposalStatus = ((Data.Enums.ProposalStatus)r.ProposalStatus).ToString(),
                LastMessage = r.LastMessage,
                LastMessageAt = r.LastMessageAt,
                UnreadCount = r.UnreadCount
            }).ToList();
        }

        public async Task<int> MarkMessagesAsReadAsync(int proposalId, string currentUserId)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    UPDATE ""Messages""
                    SET ""IsRead"" = TRUE
                    WHERE ""ProposalId"" = @ProposalId
                      AND ""SenderId"" != @CurrentUserId
                      AND ""IsRead"" = FALSE";

                var count = await connection.ExecuteAsync(sql, new
                {
                    ProposalId = proposalId,
                    CurrentUserId = currentUserId
                }, transaction);

                transaction.Commit();
                return count;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(*)
                FROM ""Messages"" m
                INNER JOIN ""Proposals"" p ON m.""ProposalId"" = p.""ProposalId""
                WHERE m.""SenderId"" != @UserId
                  AND m.""IsRead"" = FALSE
                  AND (p.""ProposerId"" = @UserId OR p.""RecipientId"" = @UserId)";

            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }

        public async Task<bool> IsProposalParticipantAsync(int proposalId, string userId)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM ""Proposals""
                WHERE ""ProposalId"" = @ProposalId
                  AND (""ProposerId"" = @UserId OR ""RecipientId"" = @UserId)";

            var count = await connection.QuerySingleAsync<int>(sql, new
            {
                ProposalId = proposalId,
                UserId = userId
            });

            return count > 0;
        }

        // Private result types for Dapper mapping
        private class ConversationQueryResult
        {
            public int ProposalId { get; set; }
            public string OtherUserId { get; set; } = string.Empty;
            public string OtherUsername { get; set; } = string.Empty;
            public string? OtherUserProfilePicture { get; set; }
            public string OfferingSkillName { get; set; } = string.Empty;
            public string ReceivingSkillName { get; set; } = string.Empty;
            public int ProposalStatus { get; set; }
            public string? LastMessage { get; set; }
            public DateTime? LastMessageAt { get; set; }
            public int UnreadCount { get; set; }
        }
    }
}
