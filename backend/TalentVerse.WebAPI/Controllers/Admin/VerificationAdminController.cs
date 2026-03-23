using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Verification;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers.Admin
{
    [Route("api/admin/verifications")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class VerificationAdminController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly ILogger<VerificationAdminController> _logger;

        public VerificationAdminController(
            IVerificationService verificationService,
            ILogger<VerificationAdminController> logger)
        {
            _verificationService = verificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of pending verification requests (Admin only)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ServiceResponse<AdminVerificationListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ServiceResponse<AdminVerificationListDto>>> GetPendingRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _verificationService.GetPendingRequestsAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get details of a specific verification request (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ServiceResponse<VerificationRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<VerificationRequestDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ServiceResponse<VerificationRequestDto>>> GetRequestById(long id)
        {
            var result = await _verificationService.GetRequestByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Approve or reject a verification request (Admin only)
        /// </summary>
        [HttpPost("{id}/review")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ServiceResponse<bool>>> ReviewRequest(long id, [FromBody] ReviewVerificationDto dto)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminUserId))
                return Unauthorized(ServiceResponse<bool>.FailureResponse("Unauthorized"));

            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.FailureResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _verificationService.ReviewRequestAsync(id, adminUserId, dto);

            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
