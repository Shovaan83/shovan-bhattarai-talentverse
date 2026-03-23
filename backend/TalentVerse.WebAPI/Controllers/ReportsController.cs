using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Admin;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IAdminService adminService, ILogger<ReportsController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Report inappropriate content (any authenticated user)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<bool>>> ReportContent([FromBody] ReportContentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(ServiceResponse<bool>.FailureResponse("Unauthorized"));

        var result = await _adminService.ReportContentAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
