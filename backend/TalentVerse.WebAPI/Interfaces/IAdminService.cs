using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Admin;

namespace TalentVerse.WebAPI.Interfaces;

public interface IAdminService
{
    Task<ServiceResponse<AdminUserListDto>> SearchUsersAsync(string? query, int page, int pageSize);
    Task<ServiceResponse<bool>> UpdateUserStatusAsync(string userId, UpdateUserStatusDto dto);
    Task<ServiceResponse<AdminDashboardDto>> GetDashboardAsync();

    // Content Moderation
    Task<ServiceResponse<bool>> ReportContentAsync(string userId, ReportContentDto dto);
    Task<ServiceResponse<FlaggedContentListDto>> GetFlaggedContentAsync(int page, int pageSize);
    Task<ServiceResponse<AdminSkillListDto>> SearchSkillsAsync(string? query, int page, int pageSize);
    Task<ServiceResponse<AdminReviewListDto>> SearchReviewsAsync(string? query, int page, int pageSize);
    Task<ServiceResponse<bool>> RemoveSkillAsync(int userSkillId, string adminId, string reason);
    Task<ServiceResponse<bool>> RemoveReviewAsync(int reviewId, string adminId, string reason);
    Task<ServiceResponse<bool>> DismissReportAsync(int reportId, string adminId);

    // Dispute Resolution
    Task<ServiceResponse<AdminProposalListDto>> SearchProposalsAsync(string? query, int? status, int page, int pageSize);
    Task<ServiceResponse<bool>> ResolveDisputeAsync(int proposalId, string adminId, ResolveDisputeDto dto);
}
