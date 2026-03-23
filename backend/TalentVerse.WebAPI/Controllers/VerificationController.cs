using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Verification;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VerificationController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(
            IVerificationService verificationService,
            ICloudinaryService cloudinaryService,
            ILogger<VerificationController> logger)
        {
            _verificationService = verificationService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's verification status
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ServiceResponse<VerificationStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<VerificationStatusDto>>> GetMyStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ServiceResponse<VerificationStatusDto>.FailureResponse("Unauthorized"));

            var result = await _verificationService.GetMyStatusAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Upload a verification document (PDF or image)
        /// </summary>
        [HttpPost("upload-document")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<object>>> UploadDocument(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ServiceResponse<object>.FailureResponse("Unauthorized"));

            var result = await _cloudinaryService.UploadVerificationDocumentAsync(file);

            if (!result.Success)
                return BadRequest(ServiceResponse<object>.FailureResponse(result.Message));

            return Ok(ServiceResponse<object>.SuccessResponse(new
            {
                url = result.Data?.Url,
                publicId = result.Data?.PublicId
            }, result.Message));
        }

        /// <summary>
        /// Submit a verification request
        /// </summary>
        [HttpPost("submit")]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<VerificationStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<VerificationStatusDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceResponse<VerificationStatusDto>>> SubmitRequest([FromBody] SubmitVerificationRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(ServiceResponse<VerificationStatusDto>.FailureResponse("Unauthorized"));

            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<VerificationStatusDto>.FailureResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _verificationService.SubmitRequestAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
