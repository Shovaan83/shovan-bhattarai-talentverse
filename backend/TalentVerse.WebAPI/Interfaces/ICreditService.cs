using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Credits;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface ICreditService
    {
        Task<ServiceResponse<WalletDto>> GetWalletAsync(string userId);
        Task<ServiceResponse<TransactionListResponseDto>> GetTransactionsAsync(string userId, TransactionFilterDto filter);
        Task<ServiceResponse<LeaderboardResponseDto>> GetLeaderboardAsync(string currentUserId);
        Task<ServiceResponse<IEnumerable<CreditPackDto>>> GetCreditPacksAsync();
        Task<ServiceResponse<CheckoutSessionDto>> CreateCheckoutSessionAsync(string userId, string packId, string successUrl, string cancelUrl);
        Task<ServiceResponse<bool>> HandleStripeWebhookAsync(string json, string stripeSignature);

        // Internal methods called by other services
        Task AwardSignupBonusAsync(string userId);
        Task AwardSwapRewardAsync(string proposerId, string recipientId, long proposalId);
        Task AwardBadgeRewardAsync(string userId, decimal amount, string badgeName);
        Task<decimal> GetBalanceAsync(string userId);
    }
}
