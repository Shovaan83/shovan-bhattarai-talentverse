using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class BadgesController : ControllerBase
{
    private readonly IBadgeService _badgeService;
    private readonly ILogger<BadgesController> _logger;

    public BadgesController(IBadgeService badgeService, ILogger<BadgesController> logger)
    {
        _badgeService = badgeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all badges, each annotated with whether the current user has earned it
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllBadges()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _badgeService.GetAllBadgesAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get only the badges the current user has earned
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyBadges()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _badgeService.GetUserBadgesAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get badges for a specific user by ID (public profile view)
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserBadges(string userId)
    {
        var result = await _badgeService.GetUserBadgesAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
