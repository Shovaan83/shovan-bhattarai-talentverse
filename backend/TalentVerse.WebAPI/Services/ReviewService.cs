using Microsoft.AspNetCore.Identity;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Reviews;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IReviewRepository reviewRepository,
        IProposalRepository proposalRepository,
        UserManager<AppUser> userManager,
        ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _proposalRepository = proposalRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ServiceResponse<ReviewDto>> CreateReviewAsync(string userId, CreateReviewDto dto)
    {
        try
        {
            // 1. Guard clauses
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            // 2. Fail-fast validation (cheap checks)
            if (dto.Rating < 1 || dto.Rating > 5)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.InvalidRating);

            if (!string.IsNullOrWhiteSpace(dto.Comment) && dto.Comment.Length > 500)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.CommentTooLong);

            // 3. Business rule validation (expensive - database queries)
            // Check if proposal exists
            var proposal = await _proposalRepository.GetEntityByIdAsync(dto.ProposalId);
            if (proposal == null)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

            // Check if proposal is completed
            if (proposal.Status != ProposalStatus.Completed)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.ProposalNotCompleted);

            // Check if user is a participant in the proposal
            bool isParticipant = proposal.ProposerId == userId || proposal.RecipientId == userId;
            if (!isParticipant)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.NotProposalParticipant);

            // Check if user has already reviewed this proposal (immutable reviews)
            var hasReviewed = await _reviewRepository.HasUserReviewedProposalAsync(userId, dto.ProposalId);
            if (hasReviewed)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.AlreadyReviewedProposal);

            // Determine who is being reviewed (the other party)
            string revieweeId = proposal.ProposerId == userId ? proposal.RecipientId : proposal.ProposerId;

            // Verify reviewee exists
            var reviewee = await _userManager.FindByIdAsync(revieweeId);
            if (reviewee == null)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.UserNotFound);

            // 4. Execute operation
            var review = new Review
            {
                ProposalId = dto.ProposalId,
                ReviewerId = userId,
                RevieweeId = revieweeId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var created = await _reviewRepository.AddReviewAsync(review);

            // 5. Verify success
            if (created == null)
                return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.ReviewCreationFailed);

            // 6. Get reviewer info for response
            var reviewer = await _userManager.FindByIdAsync(userId);
            
            // 7. Return result
            var reviewDto = new ReviewDto
            {
                ReviewId = created.ReviewId,
                ProposalId = created.ProposalId,
                ReviewerId = created.ReviewerId,
                ReviewerUsername = reviewer?.UserName ?? string.Empty,
                ReviewerProfilePictureUrl = reviewer?.ProfilePictureURL ?? string.Empty,
                RevieweeId = created.RevieweeId,
                RevieweeUsername = reviewee.UserName ?? string.Empty,
                Rating = created.Rating,
                Comment = created.Comment,
                CreatedAt = created.CreatedAt
            };

            _logger.LogInformation("User {UserId} submitted review for proposal {ProposalId}", userId, dto.ProposalId);

            return ServiceResponse<ReviewDto>.SuccessResponse(reviewDto, AppConstant.SuccessMessages.ReviewSubmitted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for user {UserId} on proposal {ProposalId}", userId, dto.ProposalId);
            return ServiceResponse<ReviewDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<IEnumerable<ReviewDto>>> GetReviewsByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<IEnumerable<ReviewDto>>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return ServiceResponse<IEnumerable<ReviewDto>>.SuccessResponse(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reviews for user {UserId}", userId);
            return ServiceResponse<IEnumerable<ReviewDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<IEnumerable<ReviewDto>>> GetReviewsForProposalAsync(int proposalId)
    {
        try
        {
            if (proposalId <= 0)
                return ServiceResponse<IEnumerable<ReviewDto>>.FailureResponse(AppConstant.ErrorMessages.InvalidProposalId);

            var reviews = await _reviewRepository.GetReviewsForProposalAsync(proposalId);
            return ServiceResponse<IEnumerable<ReviewDto>>.SuccessResponse(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reviews for proposal {ProposalId}", proposalId);
            return ServiceResponse<IEnumerable<ReviewDto>>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> CanUserReviewProposalAsync(string userId, int proposalId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            if (proposalId <= 0)
                return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.InvalidProposalId);

            // Check if proposal exists and is completed
            var proposal = await _proposalRepository.GetEntityByIdAsync(proposalId);
            if (proposal == null)
                return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.ProposalNotFound);

            if (proposal.Status != ProposalStatus.Completed)
                return ServiceResponse<bool>.SuccessResponse(false, "Proposal is not completed yet");

            // Check if user is a participant
            bool isParticipant = proposal.ProposerId == userId || proposal.RecipientId == userId;
            if (!isParticipant)
                return ServiceResponse<bool>.SuccessResponse(false, "User is not a participant in this proposal");

            // Check if user has already reviewed
            var hasReviewed = await _reviewRepository.HasUserReviewedProposalAsync(userId, proposalId);
            if (hasReviewed)
                return ServiceResponse<bool>.SuccessResponse(false, "User has already reviewed this proposal");

            return ServiceResponse<bool>.SuccessResponse(true, "User can review this proposal");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can review proposal {ProposalId}", userId, proposalId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<UserReputationDto>> GetUserReputationAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<UserReputationDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<UserReputationDto>.FailureResponse(AppConstant.ErrorMessages.UserNotFound);

            var reputation = await _reviewRepository.GetUserReputationAsync(userId);
            
            if (reputation == null)
            {
                // Return default reputation if no reviews exist
                reputation = new UserReputationDto
                {
                    UserId = userId,
                    AverageRating = 0,
                    TotalReviews = 0,
                    CompletedSwaps = 0
                };
            }

            return ServiceResponse<UserReputationDto>.SuccessResponse(reputation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching reputation for user {UserId}", userId);
            return ServiceResponse<UserReputationDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }
}
