using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Skills;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepo;
    private readonly ILogger<SkillService> _logger;

    public SkillService(ISkillRepository skillRepo, ILogger<SkillService> logger)
    {
        _skillRepo = skillRepo;
        _logger = logger;
    }

    public async Task<ServiceResponse<bool>> AddSkillAsync(string userId, AddSkillDto skillDto)
    {
        try
        {
            // Guard clause: null or empty userId
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("{Method} called with null or empty userId", nameof(AddSkillAsync));
                return ServiceResponse<bool>.FailureResponse("User ID is required.");
            }

            // Guard clause: null DTO
            if (skillDto is null)
                return ServiceResponse<bool>.FailureResponse("Skill data is required.");

            // Input sanitization
            var trimmedSkillName = skillDto.SkillName?.Trim();
            var trimmedCategory = skillDto.Category?.Trim();
            var trimmedDescription = skillDto.Description?.Trim();

            // Fail-fast: cheap validations
            if (string.IsNullOrWhiteSpace(trimmedSkillName))
                return ServiceResponse<bool>.FailureResponse("Skill name is required.");

            if (trimmedSkillName.Length < 2 || trimmedSkillName.Length > 100)
                return ServiceResponse<bool>.FailureResponse("Skill name must be between 2 and 100 characters.");

            if (string.IsNullOrWhiteSpace(trimmedCategory))
                return ServiceResponse<bool>.FailureResponse("Skill category is required.");

            if (trimmedCategory.Length < 2 || trimmedCategory.Length > 50)
                return ServiceResponse<bool>.FailureResponse("Skill category must be between 2 and 50 characters.");

            if (skillDto.Type < 0 || skillDto.Type > 1)
                return ServiceResponse<bool>.FailureResponse("Skill type must be 0 (Offered) or 1 (Wanted).");

            if (!string.IsNullOrEmpty(trimmedDescription) && trimmedDescription.Length > 500)
                return ServiceResponse<bool>.FailureResponse("Description cannot exceed 500 characters.");

            // Sanitize the DTO (update for repository)
            skillDto.SkillName = trimmedSkillName;
            skillDto.Category = trimmedCategory;
            skillDto.Description = string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription;

            // Expensive operation: database call
            var success = await _skillRepo.AddSkillToUserAsync(userId, skillDto);

            if (success)
            {
                _logger.LogInformation("Skill '{SkillName}' added for user {UserId}", trimmedSkillName, userId);
                return ServiceResponse<bool>.SuccessResponse(true, "Skill added successfully.");
            }

            _logger.LogWarning("Failed to add skill '{SkillName}' for user {UserId}", trimmedSkillName, userId);
            return ServiceResponse<bool>.FailureResponse("Failed to add skill. It may already exist or an error occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for user {UserId}", nameof(AddSkillAsync), userId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<IEnumerable<SkillDto>>> GetUserSkillsAsync(string userId)
    {
        try
        {
            // Guard clause: null or empty userId
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("{Method} called with null or empty userId", nameof(GetUserSkillsAsync));
                return ServiceResponse<IEnumerable<SkillDto>>.FailureResponse("User ID is required.");
            }

            // Expensive operation: database call
            var skills = await _skillRepo.GetUserSkillsAsync(userId);

            _logger.LogInformation("Retrieved {Count} skills for user {UserId}", skills?.Count() ?? 0, userId);
            return ServiceResponse<IEnumerable<SkillDto>>.SuccessResponse(skills ?? Enumerable.Empty<SkillDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for user {UserId}", nameof(GetUserSkillsAsync), userId);
            return ServiceResponse<IEnumerable<SkillDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> DeleteSkillAsync(string userId, int userSkillId)
    {
        try
        {
            // Guard clause: null or empty userId
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("{Method} called with null or empty userId", nameof(DeleteSkillAsync));
                return ServiceResponse<bool>.FailureResponse("User ID is required.");
            }

            // Guard clause: validate userSkillId
            if (userSkillId <= 0)
                return ServiceResponse<bool>.FailureResponse("Invalid skill ID. ID must be a positive number.");

            // Expensive operation: database call
            var success = await _skillRepo.DeleteUserSkillAsync(userId, userSkillId);

            if (success)
            {
                _logger.LogInformation("Skill {SkillId} deleted for user {UserId}", userSkillId, userId);
                return ServiceResponse<bool>.SuccessResponse(true, "Skill deleted successfully.");
            }

            _logger.LogWarning("Failed to delete skill {SkillId} for user {UserId} - skill not found or unauthorized", userSkillId, userId);
            return ServiceResponse<bool>.FailureResponse("Skill not found or you don't have permission to delete it.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for skill {SkillId} and user {UserId}", nameof(DeleteSkillAsync), userSkillId, userId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }
}