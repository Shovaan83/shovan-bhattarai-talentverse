using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Proposals;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProposalsController : ControllerBase
    {
        private readonly IProposalService _proposalService;
        private readonly ILogger<ProposalsController> _logger;

        public ProposalsController(IProposalService proposalService, ILogger<ProposalsController> logger)
        {
            _proposalService = proposalService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new swap proposal
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalDto>>> CreateProposal([FromBody] CreateProposalDto dto)
        {
            if (dto == null)
                return BadRequest(ServiceResponse<ProposalDto>.FailureResponse("Request body is required."));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ServiceResponse<ProposalDto>.FailureResponse("Validation failed.", errors));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalDto>.FailureResponse("User authentication failed."));

            var result = await _proposalService.CreateProposalAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get paginated list of user's proposals
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ServiceResponse<ProposalListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalListResponseDto>>> GetProposals(
            [FromQuery] string? direction = null,
            [FromQuery] string? status = null,
            [FromQuery] string? searchQuery = null,
            [FromQuery] string? sortBy = "UpdatedAt",
            [FromQuery] string? sortOrder = "desc",
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalListResponseDto>.FailureResponse("User authentication failed."));

            var filter = new ProposalFilterDto
            {
                Direction = direction,
                Status = status,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortOrder = sortOrder,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Page = page,
                PageSize = pageSize
            };

            var result = await _proposalService.GetUserProposalsAsync(userId, filter);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get a single proposal by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalDto>>> GetProposal(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalDto>.FailureResponse("User authentication failed."));

            var result = await _proposalService.GetProposalAsync(userId, id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Accept a proposal (recipient only)
        /// </summary>
        [HttpPatch("{id:int}/accept")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalDto>>> AcceptProposal(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalDto>.FailureResponse("User authentication failed."));

            var result = await _proposalService.AcceptProposalAsync(userId, id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Decline a proposal (recipient only)
        /// </summary>
        [HttpPatch("{id:int}/decline")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalDto>>> DeclineProposal(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalDto>.FailureResponse("User authentication failed."));

            var result = await _proposalService.DeclineProposalAsync(userId, id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Cancel a proposal (proposer only)
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalDto>>> CancelProposal(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalDto>.FailureResponse("User authentication failed."));

            var result = await _proposalService.CancelProposalAsync(userId, id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Confirm completion of a swap (both parties must confirm)
        /// </summary>
        [HttpPatch("{id:int}/confirm-completion")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<ProposalDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<ProposalDto>>> ConfirmCompletion(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<ProposalDto>.FailureResponse("User authentication failed."));

            var result = await _proposalService.ConfirmCompletionAsync(userId, id);

            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
