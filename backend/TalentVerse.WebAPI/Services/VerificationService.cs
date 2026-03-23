using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Verification;
using TalentVerse.WebAPI.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace TalentVerse.WebAPI.Services;

public class VerificationService : IVerificationService
{
    private readonly IVerificationRepository _verificationRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IBadgeService _badgeService;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(
        IVerificationRepository verificationRepository,
        ICloudinaryService cloudinaryService,
        IBadgeService badgeService,
        IEmailService emailService,
        UserManager<AppUser> userManager,
        ILogger<VerificationService> logger)
    {
        _verificationRepository = verificationRepository;
        _cloudinaryService = cloudinaryService;
        _badgeService = badgeService;
        _emailService = emailService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ServiceResponse<VerificationStatusDto>> GetMyStatusAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<VerificationStatusDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            var status = await _verificationRepository.GetUserVerificationStatusAsync(userId);
            return ServiceResponse<VerificationStatusDto>.SuccessResponse(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification status for user {UserId}", userId);
            return ServiceResponse<VerificationStatusDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<VerificationStatusDto>> SubmitRequestAsync(string userId, SubmitVerificationRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<VerificationStatusDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            if (string.IsNullOrWhiteSpace(dto.DocumentUrl))
                return ServiceResponse<VerificationStatusDto>.FailureResponse("Document URL is required.");

            // Check if user is already verified
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<VerificationStatusDto>.FailureResponse("User not found.");

            if (user.IsIdentityVerified)
                return ServiceResponse<VerificationStatusDto>.FailureResponse("You are already verified.");

            // Check if user has a pending request
            if (await _verificationRepository.HasPendingRequestAsync(userId))
                return ServiceResponse<VerificationStatusDto>.FailureResponse("You already have a pending verification request.");

            // Create the verification request
            var request = new VerificationRequest
            {
                UserId = userId,
                DocumentUrl = dto.DocumentUrl,
                DocumentPublicId = dto.DocumentPublicId,
                Status = VerificationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            var created = await _verificationRepository.CreateRequestAsync(request);
            if (created == null)
                return ServiceResponse<VerificationStatusDto>.FailureResponse("Failed to submit verification request.");

            // Send confirmation email
            try
            {
                await _emailService.SendVerificationSubmittedAsync(user.Email!, user.UserName!);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send verification submitted email to {Email}", user.Email);
            }

            _logger.LogInformation("User {UserId} submitted verification request {RequestId}", userId, created.VerificationRequestId);

            var status = new VerificationStatusDto
            {
                Status = "Pending",
                IsVerified = false,
                SubmittedAt = created.SubmittedAt
            };

            return ServiceResponse<VerificationStatusDto>.SuccessResponse(status, "Verification request submitted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting verification request for user {UserId}", userId);
            return ServiceResponse<VerificationStatusDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<AdminVerificationListDto>> GetPendingRequestsAsync(int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (requests, totalCount) = await _verificationRepository.GetPendingRequestsAsync(page, pageSize);

            foreach (var request in requests)
            {
                request.DocumentUrl = _cloudinaryService.GenerateSecureDocumentUrl(request.DocumentUrl, request.DocumentPublicId);
            }

            var result = new AdminVerificationListDto
            {
                Requests = requests,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ServiceResponse<AdminVerificationListDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending verification requests");
            return ServiceResponse<AdminVerificationListDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<VerificationRequestDto>> GetRequestByIdAsync(long id)
    {
        try
        {
            var request = await _verificationRepository.GetByIdAsync(id);
            if (request == null)
                return ServiceResponse<VerificationRequestDto>.FailureResponse("Verification request not found.");

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return ServiceResponse<VerificationRequestDto>.FailureResponse("User not found.");

            string? reviewerName = null;
            if (!string.IsNullOrEmpty(request.ReviewedByUserId))
            {
                var reviewer = await _userManager.FindByIdAsync(request.ReviewedByUserId);
                reviewerName = reviewer?.UserName;
            }

            var dto = new VerificationRequestDto
            {
                Id = request.VerificationRequestId,
                UserId = request.UserId,
                UserName = user.UserName ?? "",
                UserEmail = user.Email ?? "",
                UserProfilePictureUrl = user.ProfilePictureURL,
                DocumentUrl = _cloudinaryService.GenerateSecureDocumentUrl(request.DocumentUrl, request.DocumentPublicId),
                Status = request.Status.ToString(),
                SubmittedAt = request.SubmittedAt,
                ReviewedAt = request.ReviewedAt,
                ReviewedByUserName = reviewerName,
                AdminNotes = request.AdminNotes,
                RejectionReason = request.RejectionReason
            };

            return ServiceResponse<VerificationRequestDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification request {Id}", id);
            return ServiceResponse<VerificationRequestDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> ReviewRequestAsync(long id, string adminUserId, ReviewVerificationDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(adminUserId))
                return ServiceResponse<bool>.FailureResponse("Admin user ID is required.");

            var request = await _verificationRepository.GetByIdAsync(id);
            if (request == null)
                return ServiceResponse<bool>.FailureResponse("Verification request not found.");

            if (request.Status != VerificationStatus.Pending)
                return ServiceResponse<bool>.FailureResponse("This request has already been reviewed.");

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return ServiceResponse<bool>.FailureResponse("User not found.");

            // Validate rejection reason if rejecting
            if (!dto.IsApproved && string.IsNullOrWhiteSpace(dto.RejectionReason))
                return ServiceResponse<bool>.FailureResponse("Rejection reason is required when rejecting a request.");

            // Update the request
            request.Status = dto.IsApproved ? VerificationStatus.Approved : VerificationStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = adminUserId;
            request.AdminNotes = dto.AdminNotes;
            request.RejectionReason = dto.IsApproved ? null : dto.RejectionReason;

            var updated = await _verificationRepository.UpdateRequestAsync(request);
            if (!updated)
                return ServiceResponse<bool>.FailureResponse("Failed to update verification request.");

            if (dto.IsApproved)
            {
                // Update user verification status
                await _verificationRepository.UpdateUserVerificationStatusAsync(
                    request.UserId,
                    true,
                    DateTime.UtcNow);

                // Award the Verified badge
                await _badgeService.AwardVerifiedBadgeAsync(request.UserId);

                // Send approval email
                try
                {
                    await _emailService.SendVerificationApprovedAsync(user.Email!, user.UserName!);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send verification approved email to {Email}", user.Email);
                }

                _logger.LogInformation(
                    "Admin {AdminId} approved verification request {RequestId} for user {UserId}",
                    adminUserId, id, request.UserId);

                return ServiceResponse<bool>.SuccessResponse(true, "Verification approved successfully.");
            }
            else
            {
                // Send rejection email
                try
                {
                    await _emailService.SendVerificationRejectedAsync(user.Email!, user.UserName!, dto.RejectionReason!);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send verification rejected email to {Email}", user.Email);
                }

                _logger.LogInformation(
                    "Admin {AdminId} rejected verification request {RequestId} for user {UserId}. Reason: {Reason}",
                    adminUserId, id, request.UserId, dto.RejectionReason);

                return ServiceResponse<bool>.SuccessResponse(true, "Verification rejected.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing verification request {Id}", id);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }
}
