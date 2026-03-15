using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TalentVerse.WebAPI.DTO.Messages;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IServiceScopeFactory scopeFactory, ILogger<ChatHub> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                // Join a personal group so unread count updates can be targeted
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} connected to ChatHub (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("User {UserId} disconnected from ChatHub", userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join the SignalR group for a specific proposal's chat
        /// </summary>
        public async Task JoinProposal(int proposalId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Not authenticated.");
            }

            using var scope = _scopeFactory.CreateScope();
            var messageRepo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

            var isParticipant = await messageRepo.IsProposalParticipantAsync(proposalId, userId);
            if (!isParticipant)
            {
                throw new HubException("You are not a participant in this proposal.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"proposal_{proposalId}");
            _logger.LogInformation("User {UserId} joined proposal group {ProposalId}", userId, proposalId);
        }

        /// <summary>
        /// Leave the SignalR group for a specific proposal's chat
        /// </summary>
        public async Task LeaveProposal(int proposalId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"proposal_{proposalId}");

            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} left proposal group {ProposalId}", userId, proposalId);
        }

        /// <summary>
        /// Send a message through the hub (delegates to MessageService for persistence + broadcast)
        /// </summary>
        public async Task SendMessage(SendMessageDto dto)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Not authenticated.");
            }

            using var scope = _scopeFactory.CreateScope();
            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

            var result = await messageService.SendMessageAsync(userId, dto);
            if (!result.Success)
            {
                throw new HubException(result.Message);
            }
            // Broadcast is handled inside MessageService via IHubContext
        }

        /// <summary>
        /// Mark messages as read in a proposal; notifies other party
        /// </summary>
        public async Task MarkAsRead(int proposalId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Not authenticated.");
            }

            using var scope = _scopeFactory.CreateScope();
            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

            var result = await messageService.MarkAsReadAsync(userId, proposalId);
            if (!result.Success)
            {
                throw new HubException(result.Message);
            }
        }
    }
}
