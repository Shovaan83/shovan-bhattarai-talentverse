using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TalentVerse.WebAPI.DTO.Credits;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class CreditsController : ControllerBase
{
    private readonly ICreditService _creditService;
    private readonly ILogger<CreditsController> _logger;

    public CreditsController(ICreditService creditService, ILogger<CreditsController> logger)
    {
        _creditService = creditService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's credit wallet (balance + stats)
    /// </summary>
    [HttpGet("wallet")]
    public async Task<IActionResult> GetWallet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _creditService.GetWalletAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get paginated transaction history for the current user
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _creditService.GetTransactionsAsync(userId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get the top-50 leaderboard with the current user's rank
    /// </summary>
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _creditService.GetLeaderboardAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get available credit packs for purchase
    /// </summary>
    [HttpGet("packs")]
    public async Task<IActionResult> GetCreditPacks()
    {
        var result = await _creditService.GetCreditPacksAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create a Stripe checkout session to purchase a credit pack
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _creditService.CreateCheckoutSessionAsync(
            userId, dto.PackId, dto.SuccessUrl, dto.CancelUrl);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Stripe webhook — processes completed checkout sessions (no auth required)
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [DisableRateLimiting]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _creditService.HandleStripeWebhookAsync(json, stripeSignature);
        return result.Success ? Ok() : BadRequest(result.Message);
    }
}

/// <summary>DTO for creating a checkout session</summary>
public class CreateCheckoutDto
{
    public string PackId { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
