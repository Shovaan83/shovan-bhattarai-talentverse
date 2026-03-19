using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Credits;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface ICreditRepository
    {
        Task<decimal> GetBalanceAsync(string userId);
        Task<CreditTransaction> AddTransactionAsync(CreditTransaction transaction);
        Task<bool> UpdateBalanceAsync(string userId, decimal newBalance);
        Task<TransactionListResponseDto> GetTransactionsAsync(string userId, TransactionFilterDto filter);
        Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int limit = 50);
        Task<int?> GetUserRankAsync(string userId);
        Task<int> GetCompletedSwapCountAsync(string userId);
        Task<int> GetReviewCountAsync(string userId);
        Task<int> GetSkillCountAsync(string userId);
        Task<double> GetAverageRatingAsync(string userId);
        Task<bool> HasTransactionByReferenceAsync(string referenceType, string referenceId);
    }
}
