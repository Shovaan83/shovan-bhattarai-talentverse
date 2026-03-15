using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Messages;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Send a message to the other party in a proposal
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<MessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<MessageDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<MessageDto>>> SendMessage([FromBody] SendMessageDto dto)
        {
            if (dto == null)
                return BadRequest(ServiceResponse<MessageDto>.FailureResponse("Request body is required."));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ServiceResponse<MessageDto>.FailureResponse("Validation failed.", errors));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<MessageDto>.FailureResponse("User authentication failed."));

            var result = await _messageService.SendMessageAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get paginated messages for a specific proposal
        /// </summary>
        [HttpGet("proposal/{proposalId:int}")]
        [ProducesResponseType(typeof(ServiceResponse<MessageListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<MessageListResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<MessageListResponseDto>>> GetMessages(
            int proposalId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<MessageListResponseDto>.FailureResponse("User authentication failed."));

            var result = await _messageService.GetMessagesAsync(userId, proposalId, page, pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all conversations (Accepted/Completed proposals) for the current user
        /// </summary>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(ServiceResponse<List<ConversationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<List<ConversationDto>>>> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<List<ConversationDto>>.FailureResponse("User authentication failed."));

            var result = await _messageService.GetConversationsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Mark all unread messages in a proposal as read
        /// </summary>
        [HttpPut("proposal/{proposalId:int}/read")]
        [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<int>>> MarkAsRead(int proposalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<int>.FailureResponse("User authentication failed."));

            var result = await _messageService.MarkAsReadAsync(userId, proposalId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get total unread message count across all proposals
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ServiceResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<int>>> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<int>.FailureResponse("User authentication failed."));

            var result = await _messageService.GetUnreadCountAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
