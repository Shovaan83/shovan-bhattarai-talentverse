using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Messages;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IMessageRepository
    {
        /// <summary>
        /// Insert a new message and return the full DTO with sender info
        /// </summary>
        Task<MessageDto?> CreateAsync(Message message, string currentUserId);

        /// <summary>
        /// Get paginated messages for a proposal (newest first)
        /// </summary>
        Task<(List<MessageDto> Messages, int TotalCount)> GetMessagesByProposalAsync(
            int proposalId, string currentUserId, int page, int pageSize);

        /// <summary>
        /// Get all conversations (Accepted/Completed proposals) for a user, ordered by last message time
        /// </summary>
        Task<List<ConversationDto>> GetUserConversationsAsync(string userId);

        /// <summary>
        /// Mark all messages from the other party in a proposal as read; returns count updated
        /// </summary>
        Task<int> MarkMessagesAsReadAsync(int proposalId, string currentUserId);

        /// <summary>
        /// Get total unread message count across all proposals for a user
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Check whether a user is the proposer or recipient of a proposal
        /// </summary>
        Task<bool> IsProposalParticipantAsync(int proposalId, string userId);
    }
}
