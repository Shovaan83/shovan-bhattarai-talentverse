using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Marketplace;

namespace TalentVerse.WebAPI.Interfaces;

public interface IMarketplaceService
{
    Task<ServiceResponse<UserSearchResultDto>> SearchUsersAsync(UserSearchDto searchDto, string currentUserId);
    Task<ServiceResponse<PublicUserDto>> GetUserProfileAsync(string userId);
    Task<ServiceResponse<List<PublicUserDto>>> GetFeaturedUsersAsync(string currentUserId);
    Task<ServiceResponse<List<SkillBrowseDto>>> GetPopularSkillsAsync(string? skillType = null);
    Task<ServiceResponse<List<string>>> GetCategoriesAsync();
}
