using Microsoft.AspNetCore.Identity;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.DTO.Admin;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICreditService _creditService;
    private readonly IBadgeService _badgeService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAdminRepository adminRepository,
        UserManager<AppUser> userManager,
        ICreditService creditService,
        IBadgeService badgeService,
        ILogger<AdminService> logger)
    {
        _adminRepository = adminRepository;
        _userManager = userManager;
        _creditService = creditService;
        _badgeService = badgeService;
        _logger = logger;
    }

    public async Task<ServiceResponse<AdminUserListDto>> SearchUsersAsync(string? query, int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (users, totalCount) = await _adminRepository.SearchUsersAsync(query, page, pageSize);

            var result = new AdminUserListDto
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ServiceResponse<AdminUserListDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching admin users");
            return ServiceResponse<AdminUserListDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> UpdateUserStatusAsync(string userId, UpdateUserStatusDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<bool>.FailureResponse("User ID is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<bool>.FailureResponse("User not found.");

            // Prevent modifying admin accounts
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return ServiceResponse<bool>.FailureResponse("Cannot modify admin accounts.");

            switch (dto.Action?.ToLower())
            {
                case "suspend":
                    if (user.DeletedAt != null)
                        return ServiceResponse<bool>.FailureResponse("Cannot suspend a banned user.");

                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    _logger.LogInformation("Admin suspended user {UserId}. Reason: {Reason}", userId, dto.Reason);
                    return ServiceResponse<bool>.SuccessResponse(true, "User suspended successfully.");

                case "unsuspend":
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    _logger.LogInformation("Admin unsuspended user {UserId}", userId);
                    return ServiceResponse<bool>.SuccessResponse(true, "User unsuspended successfully.");

                case "ban":
                    if (string.IsNullOrWhiteSpace(dto.Reason))
                        return ServiceResponse<bool>.FailureResponse("Ban reason is required.");

                    user.DeletedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    _logger.LogInformation("Admin banned user {UserId}. Reason: {Reason}", userId, dto.Reason);
                    return ServiceResponse<bool>.SuccessResponse(true, "User banned successfully.");

                default:
                    return ServiceResponse<bool>.FailureResponse("Invalid action. Use 'Suspend', 'Unsuspend', or 'Ban'.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for {UserId}", userId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<AdminDashboardDto>> GetDashboardAsync()
    {
        try
        {
            var data = await _adminRepository.GetDashboardDataAsync();
            return ServiceResponse<AdminDashboardDto>.SuccessResponse(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin dashboard data");
            return ServiceResponse<AdminDashboardDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    // ───────── Content Moderation ─────────

    public async Task<ServiceResponse<bool>> ReportContentAsync(string userId, ReportContentDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.ContentType) || (dto.ContentType != "Skill" && dto.ContentType != "Review"))
                return ServiceResponse<bool>.FailureResponse("ContentType must be 'Skill' or 'Review'.");

            if (dto.ContentId <= 0)
                return ServiceResponse<bool>.FailureResponse("ContentId is required.");

            if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Length < 5)
                return ServiceResponse<bool>.FailureResponse("Reason must be at least 5 characters.");

            await _adminRepository.CreateReportAsync(userId, dto);
            _logger.LogInformation("User {UserId} reported {ContentType} #{ContentId}", userId, dto.ContentType, dto.ContentId);
            return ServiceResponse<bool>.SuccessResponse(true, "Report submitted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content report");
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<FlaggedContentListDto>> GetFlaggedContentAsync(int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (reports, totalCount) = await _adminRepository.GetFlaggedContentAsync(page, pageSize);
            var result = new FlaggedContentListDto
            {
                Reports = reports,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return ServiceResponse<FlaggedContentListDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flagged content");
            return ServiceResponse<FlaggedContentListDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<AdminSkillListDto>> SearchSkillsAsync(string? query, int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (skills, totalCount) = await _adminRepository.SearchSkillsAsync(query, page, pageSize);
            var result = new AdminSkillListDto
            {
                Skills = skills,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return ServiceResponse<AdminSkillListDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching skills for moderation");
            return ServiceResponse<AdminSkillListDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<AdminReviewListDto>> SearchReviewsAsync(string? query, int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (reviews, totalCount) = await _adminRepository.SearchReviewsAsync(query, page, pageSize);
            var result = new AdminReviewListDto
            {
                Reviews = reviews,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return ServiceResponse<AdminReviewListDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching reviews for moderation");
            return ServiceResponse<AdminReviewListDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> RemoveSkillAsync(int userSkillId, string adminId, string reason)
    {
        try
        {
            var deleted = await _adminRepository.DeleteUserSkillAsync(userSkillId);
            if (!deleted)
                return ServiceResponse<bool>.FailureResponse("Skill not found.");

            // Auto-resolve any pending reports for this skill
            await _adminRepository.ResolveReportsForContentAsync("Skill", userSkillId, adminId);

            _logger.LogInformation("Admin {AdminId} removed skill {UserSkillId}. Reason: {Reason}", adminId, userSkillId, reason);
            return ServiceResponse<bool>.SuccessResponse(true, "Skill removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing skill {UserSkillId}", userSkillId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> RemoveReviewAsync(int reviewId, string adminId, string reason)
    {
        try
        {
            var deleted = await _adminRepository.DeleteReviewAsync(reviewId);
            if (!deleted)
                return ServiceResponse<bool>.FailureResponse("Review not found.");

            // Auto-resolve any pending reports for this review
            await _adminRepository.ResolveReportsForContentAsync("Review", reviewId, adminId);

            _logger.LogInformation("Admin {AdminId} removed review {ReviewId}. Reason: {Reason}", adminId, reviewId, reason);
            return ServiceResponse<bool>.SuccessResponse(true, "Review removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing review {ReviewId}", reviewId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> DismissReportAsync(int reportId, string adminId)
    {
        try
        {
            var dismissed = await _adminRepository.DismissReportAsync(reportId, adminId);
            if (!dismissed)
                return ServiceResponse<bool>.FailureResponse("Report not found or already resolved.");

            _logger.LogInformation("Admin {AdminId} dismissed report {ReportId}", adminId, reportId);
            return ServiceResponse<bool>.SuccessResponse(true, "Report dismissed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing report {ReportId}", reportId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    // ───────── Dispute Resolution ─────────

    public async Task<ServiceResponse<AdminProposalListDto>> SearchProposalsAsync(string? query, int? status, int page, int pageSize)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (proposals, totalCount) = await _adminRepository.SearchProposalsAsync(query, status, page, pageSize);
            var result = new AdminProposalListDto
            {
                Proposals = proposals,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return ServiceResponse<AdminProposalListDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching proposals for disputes");
            return ServiceResponse<AdminProposalListDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> ResolveDisputeAsync(int proposalId, string adminId, ResolveDisputeDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Action) || (dto.Action != "ForceComplete" && dto.Action != "ForceCancel"))
                return ServiceResponse<bool>.FailureResponse("Action must be 'ForceComplete' or 'ForceCancel'.");

            if (string.IsNullOrWhiteSpace(dto.AdminNote) || dto.AdminNote.Length < 5)
                return ServiceResponse<bool>.FailureResponse("Admin note must be at least 5 characters.");

            var proposal = await _adminRepository.GetProposalForAdminAsync(proposalId);
            if (proposal == null)
                return ServiceResponse<bool>.FailureResponse("Proposal not found.");

            if (proposal.Status == "Completed")
                return ServiceResponse<bool>.FailureResponse("Cannot resolve an already completed proposal.");

            if (proposal.Status == "Cancelled")
                return ServiceResponse<bool>.FailureResponse("Cannot resolve an already cancelled proposal.");

            if (dto.Action == "ForceComplete")
            {
                // Only Accepted proposals can be force-completed
                if (proposal.Status != "Accepted")
                    return ServiceResponse<bool>.FailureResponse("Only Accepted proposals can be force-completed.");

                // Set status to Completed + both confirmed
                var updated = await _adminRepository.ForceUpdateProposalStatusAsync(proposalId, 3);
                if (!updated)
                    return ServiceResponse<bool>.FailureResponse("Failed to update proposal.");

                // Award credits + badges to both parties (fire-and-forget, never block)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _creditService.AwardSwapRewardAsync(proposal.ProposerId, proposal.RecipientId, proposalId);
                        await _badgeService.EvaluateOnSwapCompletedAsync(proposal.ProposerId);
                        await _badgeService.EvaluateOnSwapCompletedAsync(proposal.RecipientId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Post-ForceComplete rewards failed for proposal {ProposalId}", proposalId);
                    }
                });

                _logger.LogInformation(
                    "Admin {AdminId} force-completed proposal {ProposalId}. Note: {AdminNote}",
                    adminId, proposalId, dto.AdminNote);

                return ServiceResponse<bool>.SuccessResponse(true, "Proposal force-completed. Credits and badges awarded.");
            }
            else // ForceCancel
            {
                var updated = await _adminRepository.ForceUpdateProposalStatusAsync(proposalId, 4);
                if (!updated)
                    return ServiceResponse<bool>.FailureResponse("Failed to update proposal.");

                _logger.LogInformation(
                    "Admin {AdminId} force-cancelled proposal {ProposalId}. Note: {AdminNote}",
                    adminId, proposalId, dto.AdminNote);

                return ServiceResponse<bool>.SuccessResponse(true, "Proposal force-cancelled.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving dispute for proposal {ProposalId}", proposalId);
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }
}
