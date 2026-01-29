using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Marketplace;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly IMarketplaceRepository _marketplaceRepository;
    private readonly ILogger<MarketplaceService> _logger;

    public MarketplaceService(
        IMarketplaceRepository marketplaceRepository,
        ILogger<MarketplaceService> logger)
    {
        _marketplaceRepository = marketplaceRepository;
        _logger = logger;
    }

    public async Task<ServiceResponse<UserSearchResultDto>> SearchUsersAsync(UserSearchDto searchDto, string currentUserId)
    {
        try
        {
            // Validate pagination
            if (searchDto.Page < 1) searchDto.Page = 1;
            if (searchDto.PageSize < 1) searchDto.PageSize = 12;
            if (searchDto.PageSize > 50) searchDto.PageSize = 50; // Max page size

            // Validate proficiency range
            if (searchDto.MinProficiency.HasValue && (searchDto.MinProficiency < 1 || searchDto.MinProficiency > 5))
            {
                return ServiceResponse<UserSearchResultDto>.FailureResponse("Minimum proficiency must be between 1 and 5");
            }
            if (searchDto.MaxProficiency.HasValue && (searchDto.MaxProficiency < 1 || searchDto.MaxProficiency > 5))
            {
                return ServiceResponse<UserSearchResultDto>.FailureResponse("Maximum proficiency must be between 1 and 5");
            }

            // Validate skill type
            if (!string.IsNullOrWhiteSpace(searchDto.SkillType) && 
                searchDto.SkillType != "Offered" && searchDto.SkillType != "Wanted")
            {
                return ServiceResponse<UserSearchResultDto>.FailureResponse("Skill type must be 'Offered' or 'Wanted'");
            }

            var (users, totalCount) = await _marketplaceRepository.SearchUsersAsync(searchDto, currentUserId);

            var result = new UserSearchResultDto
            {
                Users = users,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            };

            return ServiceResponse<UserSearchResultDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query: {Query}. Exception: {Message}", searchDto.Query, ex.Message);
            return ServiceResponse<UserSearchResultDto>.FailureResponse($"Search failed: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<PublicUserDto>> GetUserProfileAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResponse<PublicUserDto>.FailureResponse("User ID is required");
            }

            var user = await _marketplaceRepository.GetUserProfileAsync(userId);

            if (user == null)
            {
                return ServiceResponse<PublicUserDto>.FailureResponse("User not found");
            }

            return ServiceResponse<PublicUserDto>.SuccessResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
            return ServiceResponse<PublicUserDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<List<PublicUserDto>>> GetFeaturedUsersAsync(string currentUserId)
    {
        try
        {
            var users = await _marketplaceRepository.GetFeaturedUsersAsync(currentUserId);
            return ServiceResponse<List<PublicUserDto>>.SuccessResponse(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured users");
            return ServiceResponse<List<PublicUserDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<List<SkillBrowseDto>>> GetPopularSkillsAsync(string? skillType = null)
    {
        try
        {
            // Validate skill type if provided
            if (!string.IsNullOrWhiteSpace(skillType) && 
                skillType != "Offered" && skillType != "Wanted")
            {
                return ServiceResponse<List<SkillBrowseDto>>.FailureResponse("Skill type must be 'Offered' or 'Wanted'");
            }

            var skills = await _marketplaceRepository.GetPopularSkillsAsync(skillType);
            return ServiceResponse<List<SkillBrowseDto>>.SuccessResponse(skills);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular skills");
            return ServiceResponse<List<SkillBrowseDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }
}
