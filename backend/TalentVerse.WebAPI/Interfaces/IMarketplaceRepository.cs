using TalentVerse.WebAPI.DTO.Marketplace;

namespace TalentVerse.WebAPI.Interfaces;

public interface IMarketplaceRepository
{
    Task<(List<PublicUserDto> Users, int TotalCount)> SearchUsersAsync(UserSearchDto searchDto, string? excludeUserId = null);
    Task<PublicUserDto?> GetUserProfileAsync(string userId);
    Task<List<PublicUserDto>> GetFeaturedUsersAsync(string? excludeUserId = null, int limit = 12);
    Task<List<SkillBrowseDto>> GetPopularSkillsAsync(string? skillType = null, int limit = 20);
    Task<int> GetCompletedSwapsCountAsync(string userId);
}
