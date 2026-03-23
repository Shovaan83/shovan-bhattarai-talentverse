using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Verification;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface IVerificationRepository
    {
        Task<VerificationRequest?> CreateRequestAsync(VerificationRequest request);
        Task<VerificationRequest?> GetByIdAsync(long id);
        Task<VerificationRequest?> GetLatestByUserIdAsync(string userId);
        Task<(List<VerificationRequestDto> Requests, int TotalCount)> GetPendingRequestsAsync(int page, int pageSize);
        Task<bool> UpdateRequestAsync(VerificationRequest request);
        Task<VerificationStatusDto> GetUserVerificationStatusAsync(string userId);
        Task<bool> HasPendingRequestAsync(string userId);
        Task<bool> UpdateUserVerificationStatusAsync(string userId, bool isVerified, DateTime? verifiedAt);
    }
}
