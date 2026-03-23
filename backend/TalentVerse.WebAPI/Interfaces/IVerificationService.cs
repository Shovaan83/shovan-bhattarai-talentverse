using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Verification;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IVerificationService
    {
        Task<ServiceResponse<VerificationStatusDto>> GetMyStatusAsync(string userId);
        Task<ServiceResponse<VerificationStatusDto>> SubmitRequestAsync(string userId, SubmitVerificationRequestDto dto);
        Task<ServiceResponse<AdminVerificationListDto>> GetPendingRequestsAsync(int page, int pageSize);
        Task<ServiceResponse<VerificationRequestDto>> GetRequestByIdAsync(long id);
        Task<ServiceResponse<bool>> ReviewRequestAsync(long id, string adminUserId, ReviewVerificationDto dto);
    }
}
