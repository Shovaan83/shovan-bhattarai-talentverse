using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Marketplace;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class MarketplaceController : ControllerBase
{
    private readonly IMarketplaceService _marketplaceService;
    private readonly ILogger<MarketplaceController> _logger;

    public MarketplaceController(
        IMarketplaceService marketplaceService,
        ILogger<MarketplaceController> logger)
    {
        _marketplaceService = marketplaceService;
        _logger = logger;
    }

    /// <summary>
    /// Search for users by skill or name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ServiceResponse<UserSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<UserSearchResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<UserSearchResultDto>>> SearchUsers([FromQuery] UserSearchDto searchDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ServiceResponse<UserSearchResultDto>.FailureResponse(AppConstant.ErrorMessages.GenericError));
        }

        var result = await _marketplaceService.SearchUsersAsync(searchDto, userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get public profile of a specific user
    /// </summary>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(ServiceResponse<PublicUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<PublicUserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResponse<PublicUserDto>>> GetUserProfile(string userId)
    {
        var result = await _marketplaceService.GetUserProfileAsync(userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get featured/recommended users for discovery
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(ServiceResponse<List<PublicUserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse<List<PublicUserDto>>>> GetFeaturedUsers()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ServiceResponse<List<PublicUserDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError));
        }

        var result = await _marketplaceService.GetFeaturedUsersAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Browse popular skills with user counts
    /// </summary>
    [HttpGet("skills")]
    [ProducesResponseType(typeof(ServiceResponse<List<SkillBrowseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse<List<SkillBrowseDto>>>> GetPopularSkills([FromQuery] string? type = null)
    {
        var result = await _marketplaceService.GetPopularSkillsAsync(type);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get all available skill categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ServiceResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResponse<List<string>>>> GetCategories()
    {
        var result = await _marketplaceService.GetCategoriesAsync();

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
