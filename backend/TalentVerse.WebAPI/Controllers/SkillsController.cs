using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Skills;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillService;
        private readonly ILogger<SkillsController> _logger;

        public SkillsController(ISkillService skillService, ILogger<SkillsController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new skill to the authenticated user's profile
        /// </summary>
        /// <param name="skillDto">Skill details including name, category, type, and description</param>
        /// <returns>Success status indicating whether the skill was added</returns>
        /// <response code="200">Skill added successfully</response>
        /// <response code="400">Validation failed or skill could not be added</response>
        /// <response code="401">User not authenticated</response>
        [HttpPost]
        [EnableRateLimiting("fixed")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<bool>>> AddSkill([FromBody] AddSkillDto skillDto)
        {
            // Input validation
            if (skillDto == null)
                return BadRequest(ServiceResponse<bool>.FailureResponse("Request body is required"));

            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.FailureResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            // Extract user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized(ServiceResponse<bool>.FailureResponse("User authentication failed"));
            }

            // Call service
            var result = await _skillService.AddSkillAsync(userId, skillDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all skills for the authenticated user
        /// </summary>
        /// <returns>List of user's skills including offered and wanted skills</returns>
        /// <response code="200">Skills retrieved successfully</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet("my-skills")]
        [ProducesResponseType(typeof(ServiceResponse<IEnumerable<SkillDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ServiceResponse<IEnumerable<SkillDto>>>> GetMySkills()
        {
            // Extract user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized(ServiceResponse<IEnumerable<SkillDto>>.FailureResponse("User authentication failed"));
            }

            // Call service
            var result = await _skillService.GetUserSkillsAsync(userId);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a specific skill from the authenticated user's profile
        /// </summary>
        /// <param name="userSkillId">The ID of the user skill relationship to delete</param>
        /// <returns>Success status indicating whether the skill was deleted</returns>
        /// <response code="200">Skill deleted successfully</response>
        /// <response code="400">Skill not found or could not be deleted</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">Skill not found for this user</response>
        [HttpDelete("{userSkillId}")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteSkill(int userSkillId)
        {
            // Validate input
            if (userSkillId <= 0)
                return BadRequest(ServiceResponse<bool>.FailureResponse("Invalid skill ID"));

            // Extract user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized(ServiceResponse<bool>.FailureResponse("User authentication failed"));
            }

            // Call service
            var result = await _skillService.DeleteSkillAsync(userId, userSkillId);

            if (!result.Success)
            {
                // Check if it's a not found scenario
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
