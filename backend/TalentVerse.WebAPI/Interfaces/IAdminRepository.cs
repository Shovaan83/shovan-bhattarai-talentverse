using TalentVerse.WebAPI.DTO.Admin;

namespace TalentVerse.WebAPI.Interfaces;

public interface IAdminRepository
{
    Task<(List<AdminUserDto> Users, int TotalCount)> SearchUsersAsync(string? query, int page, int pageSize);
    Task<AdminDashboardDto> GetDashboardDataAsync();

    // Content Moderation
    Task<int> CreateReportAsync(string reporterId, ReportContentDto dto);
    Task<(List<FlaggedContentDto> Reports, int TotalCount)> GetFlaggedContentAsync(int page, int pageSize);
    Task<(List<AdminSkillDto> Skills, int TotalCount)> SearchSkillsAsync(string? query, int page, int pageSize);
    Task<(List<AdminReviewDto> Reviews, int TotalCount)> SearchReviewsAsync(string? query, int page, int pageSize);
    Task<bool> DeleteUserSkillAsync(int userSkillId);
    Task<bool> DeleteReviewAsync(int reviewId);
    Task<bool> ResolveReportsForContentAsync(string contentType, int contentId, string adminId);
    Task<bool> DismissReportAsync(int reportId, string adminId);

    // Dispute Resolution
    Task<(List<AdminProposalDto> Proposals, int TotalCount)> SearchProposalsAsync(string? query, int? status, int page, int pageSize);
    Task<AdminProposalDto?> GetProposalForAdminAsync(int proposalId);
    Task<bool> ForceUpdateProposalStatusAsync(int proposalId, int status);
}
