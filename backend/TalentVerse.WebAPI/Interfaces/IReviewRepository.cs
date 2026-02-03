using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Reviews;

namespace TalentVerse.WebAPI.Interfaces;

public interface IReviewRepository
{
    Task<Review?> AddReviewAsync(Review review);
    Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(string userId);
    Task<IEnumerable<ReviewDto>> GetReviewsForProposalAsync(int proposalId);
    Task<bool> HasUserReviewedProposalAsync(string userId, int proposalId);
    Task<UserReputationDto?> GetUserReputationAsync(string userId);
}
