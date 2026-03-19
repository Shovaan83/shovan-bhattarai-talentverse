using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Badges;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class BadgeService : IBadgeService
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly ICreditRepository _creditRepository;
    private readonly ICreditService _creditService;
    private readonly ILogger<BadgeService> _logger;

    // Badge name constants — must match seeded values exactly
    private const string BadgeWelcomeAboard = "Welcome Aboard";
    private const string BadgeFirstSwap = "First Swap";
    private const string BadgeSwapVeteran = "Swap Veteran";
    private const string BadgeSwapMaster = "Swap Master";
    private const string BadgeFirstReview = "First Review";
    private const string BadgeTopRated = "Top Rated";
    private const string BadgeCreditSaver = "Credit Saver";
    private const string BadgeCreditMogul = "Credit Mogul";
    private const string BadgeSkillSharer = "Skill Sharer";

    public BadgeService(
        IBadgeRepository badgeRepository,
        ICreditRepository creditRepository,
        ICreditService creditService,
        ILogger<BadgeService> logger)
    {
        _badgeRepository = badgeRepository;
        _creditRepository = creditRepository;
        _creditService = creditService;
        _logger = logger;
    }

    public async Task<ServiceResponse<IEnumerable<BadgeDto>>> GetAllBadgesAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<IEnumerable<BadgeDto>>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            var badges = await _badgeRepository.GetAllBadgesWithUserStatusAsync(userId);
            return ServiceResponse<IEnumerable<BadgeDto>>.SuccessResponse(badges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching badges for user {UserId}", userId);
            return ServiceResponse<IEnumerable<BadgeDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<IEnumerable<BadgeDto>>> GetUserBadgesAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<IEnumerable<BadgeDto>>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            var badges = await _badgeRepository.GetUserBadgesAsync(userId);
            return ServiceResponse<IEnumerable<BadgeDto>>.SuccessResponse(badges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user badges for user {UserId}", userId);
            return ServiceResponse<IEnumerable<BadgeDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task EvaluateOnSignupAsync(string userId)
    {
        await TryAwardBadgeAsync(userId, BadgeWelcomeAboard);
    }

    public async Task EvaluateOnSwapCompletedAsync(string userId)
    {
        var swapCount = await _creditRepository.GetCompletedSwapCountAsync(userId);

        if (swapCount == 1)
            await TryAwardBadgeAsync(userId, BadgeFirstSwap);

        if (swapCount >= 5)
            await TryAwardBadgeAsync(userId, BadgeSwapVeteran);

        if (swapCount >= 10)
            await TryAwardBadgeAsync(userId, BadgeSwapMaster);

        // Check credit milestone badges after swap reward is applied
        var balance = await _creditService.GetBalanceAsync(userId);
        if (balance >= 100)
            await TryAwardBadgeAsync(userId, BadgeCreditSaver);
        if (balance >= 500)
            await TryAwardBadgeAsync(userId, BadgeCreditMogul);
    }

    public async Task EvaluateOnReviewSubmittedAsync(string userId)
    {
        var reviewCount = await _creditRepository.GetReviewCountAsync(userId);
        if (reviewCount == 1)
            await TryAwardBadgeAsync(userId, BadgeFirstReview);

        var avgRating = await _creditRepository.GetAverageRatingAsync(userId);
        var totalReviews = await _creditRepository.GetReviewCountAsync(userId);
        if (avgRating >= 4.5 && totalReviews >= 3)
            await TryAwardBadgeAsync(userId, BadgeTopRated);
    }

    public async Task EvaluateOnSkillAddedAsync(string userId)
    {
        var skillCount = await _creditRepository.GetSkillCountAsync(userId);
        if (skillCount >= 5)
            await TryAwardBadgeAsync(userId, BadgeSkillSharer);
    }

    // Helper: try to award a badge; if not already held, grant it + its credit reward
    private async Task TryAwardBadgeAsync(string userId, string badgeName)
    {
        try
        {
            var badgeId = await _badgeRepository.GetBadgeIdByNameAsync(badgeName);
            if (badgeId == null) return;

            var alreadyHas = await _badgeRepository.UserHasBadgeAsync(userId, badgeId.Value);
            if (alreadyHas) return;

            var awarded = await _badgeRepository.AwardBadgeAsync(userId, badgeId.Value);
            if (!awarded) return;

            _logger.LogInformation("Awarded badge \"{Badge}\" to user {UserId}", badgeName, userId);

            // Fetch credit reward from the badge record
            var allBadges = await _badgeRepository.GetAllBadgesWithUserStatusAsync(userId);
            var badge = allBadges.FirstOrDefault(b => b.BadgeId == badgeId.Value);
            if (badge != null && badge.CreditReward > 0)
                await _creditService.AwardBadgeRewardAsync(userId, badge.CreditReward, badgeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awarding badge \"{Badge}\" to user {UserId}", badgeName, userId);
        }
    }
}
