using Microsoft.AspNetCore.SignalR;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Messages;
using TalentVerse.WebAPI.Hubs;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepo;
        private readonly IProposalRepository _proposalRepo;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IMessageRepository messageRepo,
            IProposalRepository proposalRepo,
            IHubContext<ChatHub> hubContext,
            ILogger<MessageService> logger)
        {
            _messageRepo = messageRepo;
            _proposalRepo = proposalRepo;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ServiceResponse<MessageDto>> SendMessageAsync(string userId, SendMessageDto dto)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                // 2. Input sanitization
                var content = dto.MessageContent?.Trim();

                // 3. Fail-fast validation
                if (string.IsNullOrWhiteSpace(content))
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.MessageEmpty);

                if (content.Length > 2000)
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.MessageTooLong);

                // 4. Business rule validation - participant check
                var isParticipant = await _messageRepo.IsProposalParticipantAsync(dto.ProposalId, userId);
                if (!isParticipant)
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.NotProposalParticipant);

                // Business rule - proposal must be Accepted or Completed
                var proposal = await _proposalRepo.GetEntityByIdAsync(dto.ProposalId);
                if (proposal == null)
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

                if (proposal.Status != ProposalStatus.Accepted && proposal.Status != ProposalStatus.Completed)
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.ChatNotAvailable);

                // 5. Execute operation
                var message = new Message
                {
                    ProposalId = dto.ProposalId,
                    SenderId = userId,
                    MessageContent = content,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                var created = await _messageRepo.CreateAsync(message, userId);

                // 6. Verify success
                if (created == null)
                    return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);

                // 7. Broadcast via SignalR to the proposal group
                await _hubContext.Clients
                    .Group($"proposal_{dto.ProposalId}")
                    .SendAsync("ReceiveMessage", created);

                // Notify recipient's personal group about updated unread count
                var recipientId = proposal.ProposerId == userId ? proposal.RecipientId : proposal.ProposerId;
                var unreadCount = await _messageRepo.GetUnreadCountAsync(recipientId);
                await _hubContext.Clients
                    .Group($"user_{recipientId}")
                    .SendAsync("UnreadCountUpdated", unreadCount);

                _logger.LogInformation("User {UserId} sent message to proposal {ProposalId}", userId, dto.ProposalId);

                return ServiceResponse<MessageDto>.SuccessResponse(created, AppConstant.SuccessMessages.MessageSent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message for user {UserId} on proposal {ProposalId}", userId, dto.ProposalId);
                return ServiceResponse<MessageDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<MessageListResponseDto>> GetMessagesAsync(
            string userId, int proposalId, int page, int pageSize)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<MessageListResponseDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                // 2. Validate pagination params
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                // 3. Business rule - participant check
                var isParticipant = await _messageRepo.IsProposalParticipantAsync(proposalId, userId);
                if (!isParticipant)
                    return ServiceResponse<MessageListResponseDto>.FailureResponse(AppConstant.ErrorMessages.NotProposalParticipant);

                // 4. Fetch messages
                var (messages, totalCount) = await _messageRepo.GetMessagesByProposalAsync(proposalId, userId, page, pageSize);

                // Auto-mark as read when fetching
                await _messageRepo.MarkMessagesAsReadAsync(proposalId, userId);

                // Notify sender's personal group about zeroed unread count for this proposal
                var updatedUnread = await _messageRepo.GetUnreadCountAsync(userId);
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("UnreadCountUpdated", updatedUnread);

                var response = new MessageListResponseDto
                {
                    Messages = messages,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    HasMore = (page * pageSize) < totalCount
                };

                return ServiceResponse<MessageListResponseDto>.SuccessResponse(response, AppConstant.SuccessMessages.MessagesFetched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching messages for user {UserId} on proposal {ProposalId}", userId, proposalId);
                return ServiceResponse<MessageListResponseDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<List<ConversationDto>>> GetConversationsAsync(string userId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<List<ConversationDto>>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                var conversations = await _messageRepo.GetUserConversationsAsync(userId);

                return ServiceResponse<List<ConversationDto>>.SuccessResponse(conversations, AppConstant.SuccessMessages.ConversationsFetched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversations for user {UserId}", userId);
                return ServiceResponse<List<ConversationDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<int>> MarkAsReadAsync(string userId, int proposalId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<int>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                // 2. Business rule - participant check
                var isParticipant = await _messageRepo.IsProposalParticipantAsync(proposalId, userId);
                if (!isParticipant)
                    return ServiceResponse<int>.FailureResponse(AppConstant.ErrorMessages.NotProposalParticipant);

                var updatedCount = await _messageRepo.MarkMessagesAsReadAsync(proposalId, userId);

                // Propagate updated unread count via SignalR
                var unreadCount = await _messageRepo.GetUnreadCountAsync(userId);
                await _hubContext.Clients
                    .Group($"user_{userId}")
                    .SendAsync("UnreadCountUpdated", unreadCount);

                // Notify other party's group that their messages were read
                await _hubContext.Clients
                    .Group($"proposal_{proposalId}")
                    .SendAsync("MessagesRead", proposalId);

                return ServiceResponse<int>.SuccessResponse(updatedCount, AppConstant.SuccessMessages.MessagesMarkedRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for user {UserId} on proposal {ProposalId}", userId, proposalId);
                return ServiceResponse<int>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }

        public async Task<ServiceResponse<int>> GetUnreadCountAsync(string userId)
        {
            try
            {
                // 1. Guard clauses
                if (string.IsNullOrWhiteSpace(userId))
                    return ServiceResponse<int>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

                var count = await _messageRepo.GetUnreadCountAsync(userId);

                return ServiceResponse<int>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread count for user {UserId}", userId);
                return ServiceResponse<int>.FailureResponse(AppConstant.ErrorMessages.GenericError);
            }
        }
    }
}
