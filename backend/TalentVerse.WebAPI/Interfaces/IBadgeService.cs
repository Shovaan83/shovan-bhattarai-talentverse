using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Badges;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IBadgeService
    {
        Task<ServiceResponse<IEnumerable<BadgeDto>>> GetAllBadgesAsync(string userId);
        Task<ServiceResponse<IEnumerable<BadgeDto>>> GetUserBadgesAsync(string userId);

        // Evaluation triggers — called after key events
        Task EvaluateOnSignupAsync(string userId);
        Task EvaluateOnSwapCompletedAsync(string userId);
        Task EvaluateOnReviewSubmittedAsync(string userId);
        Task EvaluateOnSkillAddedAsync(string userId);

        // Manual badge award — called by VerificationService
        Task AwardVerifiedBadgeAsync(string userId);
    }
}
