using TalentVerse.WebAPI.DTO.Badges;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IBadgeRepository
    {
        Task<IEnumerable<BadgeDto>> GetAllBadgesWithUserStatusAsync(string userId);
        Task<IEnumerable<BadgeDto>> GetUserBadgesAsync(string userId);
        Task<bool> UserHasBadgeAsync(string userId, int badgeId);
        Task<bool> AwardBadgeAsync(string userId, int badgeId);
        Task<int?> GetBadgeIdByNameAsync(string badgeName);
    }
}
