using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Admin;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of users with search (Admin only)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ServiceResponse<AdminUserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceResponse<AdminUserListDto>>> SearchUsers(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.SearchUsersAsync(query, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Update user status: Suspend, Unsuspend, or Ban (Admin only)
    /// </summary>
    [HttpPut("users/{userId}/status")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceResponse<bool>>> UpdateUserStatus(
        string userId,
        [FromBody] UpdateUserStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<bool>.FailureResponse(
                "Validation failed",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _adminService.UpdateUserStatusAsync(userId, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get platform analytics dashboard data (Admin only)
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ServiceResponse<AdminDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceResponse<AdminDashboardDto>>> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return Ok(result);
    }

    // ───────── Content Moderation ─────────

    /// <summary>
    /// Get flagged content queue (Admin only)
    /// </summary>
    [HttpGet("moderation/reports")]
    public async Task<ActionResult<ServiceResponse<FlaggedContentListDto>>> GetFlaggedContent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetFlaggedContentAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Browse all skills (Admin only)
    /// </summary>
    [HttpGet("moderation/skills")]
    public async Task<ActionResult<ServiceResponse<AdminSkillListDto>>> SearchSkills(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.SearchSkillsAsync(query, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Browse all reviews (Admin only)
    /// </summary>
    [HttpGet("moderation/reviews")]
    public async Task<ActionResult<ServiceResponse<AdminReviewListDto>>> SearchReviews(
        [FromQuery] string? query = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.SearchReviewsAsync(query, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Remove a skill listing (Admin only)
    /// </summary>
    [HttpDelete("moderation/skills/{userSkillId}")]
    public async Task<ActionResult<ServiceResponse<bool>>> RemoveSkill(int userSkillId, [FromBody] RemoveContentDto dto)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(adminId))
            return Unauthorized(ServiceResponse<bool>.FailureResponse("Unauthorized"));

        var result = await _adminService.RemoveSkillAsync(userSkillId, adminId, dto.Reason);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Remove a review (Admin only)
    /// </summary>
    [HttpDelete("moderation/reviews/{reviewId}")]
    public async Task<ActionResult<ServiceResponse<bool>>> RemoveReview(int reviewId, [FromBody] RemoveContentDto dto)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(adminId))
            return Unauthorized(ServiceResponse<bool>.FailureResponse("Unauthorized"));

        var result = await _adminService.RemoveReviewAsync(reviewId, adminId, dto.Reason);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Dismiss a content report (Admin only)
    /// </summary>
    [HttpPost("moderation/reports/{reportId}/dismiss")]
    public async Task<ActionResult<ServiceResponse<bool>>> DismissReport(int reportId)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(adminId))
            return Unauthorized(ServiceResponse<bool>.FailureResponse("Unauthorized"));

        var result = await _adminService.DismissReportAsync(reportId, adminId);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    // ───────── Dispute Resolution ─────────

    /// <summary>
    /// Browse proposals with optional status filter (Admin only)
    /// </summary>
    [HttpGet("disputes")]
    public async Task<ActionResult<ServiceResponse<AdminProposalListDto>>> SearchProposals(
        [FromQuery] string? query = null,
        [FromQuery] int? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.SearchProposalsAsync(query, status, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Resolve a disputed proposal: ForceComplete or ForceCancel (Admin only)
    /// </summary>
    [HttpPut("disputes/{proposalId}/resolve")]
    public async Task<ActionResult<ServiceResponse<bool>>> ResolveDispute(
        int proposalId,
        [FromBody] ResolveDisputeDto dto)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(adminId))
            return Unauthorized(ServiceResponse<bool>.FailureResponse("Unauthorized"));

        var result = await _adminService.ResolveDisputeAsync(proposalId, adminId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
