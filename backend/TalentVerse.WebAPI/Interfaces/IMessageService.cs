using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Messages;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IMessageService
    {
        Task<ServiceResponse<MessageDto>> SendMessageAsync(string userId, SendMessageDto dto);
        Task<ServiceResponse<MessageListResponseDto>> GetMessagesAsync(string userId, int proposalId, int page, int pageSize);
        Task<ServiceResponse<List<ConversationDto>>> GetConversationsAsync(string userId);
        Task<ServiceResponse<int>> MarkAsReadAsync(string userId, int proposalId);
        Task<ServiceResponse<int>> GetUnreadCountAsync(string userId);
    }
}
