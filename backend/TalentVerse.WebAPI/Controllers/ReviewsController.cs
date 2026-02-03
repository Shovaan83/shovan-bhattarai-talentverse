using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Reviews;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a review for a completed proposal
    /// </summary>
    /// <param name="dto">Review details including proposal ID, rating (1-5), and optional comment</param>
    /// <returns>The created review</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResponse<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<ReviewDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<ReviewDto>>> CreateReview([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ServiceResponse<ReviewDto>.FailureResponse("Validation failed"));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reviewService.CreateReviewAsync(userId, dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all reviews received by a specific user
    /// </summary>
    /// <param name="userId">The ID of the user whose reviews to retrieve</param>
    /// <returns>List of reviews for the user</returns>
    [HttpGet("user/{userId}")]
    [AllowAnonymous] // Public profiles can show reviews
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ReviewDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<IEnumerable<ReviewDto>>>> GetReviewsByUserId(string userId)
    {
        var result = await _reviewService.GetReviewsByUserIdAsync(userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get all reviews for a specific proposal (both parties' reviews)
    /// </summary>
    /// <param name="proposalId">The ID of the proposal</param>
    /// <returns>List of reviews for the proposal</returns>
    [HttpGet("proposal/{proposalId}")]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<IEnumerable<ReviewDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<IEnumerable<ReviewDto>>>> GetReviewsForProposal(int proposalId)
    {
        var result = await _reviewService.GetReviewsForProposalAsync(proposalId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Check if the current user can review a specific proposal
    /// </summary>
    /// <param name="proposalId">The ID of the proposal</param>
    /// <returns>Boolean indicating if user can review</returns>
    [HttpGet("can-review/{proposalId}")]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResponse<bool>>> CanUserReviewProposal(int proposalId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reviewService.CanUserReviewProposalAsync(userId, proposalId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get reputation statistics for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>User's reputation including average rating, total reviews, and completed swaps</returns>
    [HttpGet("reputation/{userId}")]
    [AllowAnonymous] // Public profiles can show reputation
    [ProducesResponseType(typeof(ServiceResponse<UserReputationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<UserReputationDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse<UserReputationDto>>> GetUserReputation(string userId)
    {
        var result = await _reviewService.GetUserReputationAsync(userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
