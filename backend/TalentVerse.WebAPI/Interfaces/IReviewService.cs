using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Reviews;

namespace TalentVerse.WebAPI.Interfaces;

public interface IReviewService
{
    Task<ServiceResponse<ReviewDto>> CreateReviewAsync(string userId, CreateReviewDto dto);
    Task<ServiceResponse<IEnumerable<ReviewDto>>> GetReviewsByUserIdAsync(string userId);
    Task<ServiceResponse<IEnumerable<ReviewDto>>> GetReviewsForProposalAsync(int proposalId);
    Task<ServiceResponse<bool>> CanUserReviewProposalAsync(string userId, int proposalId);
    Task<ServiceResponse<UserReputationDto>> GetUserReputationAsync(string userId);
}
