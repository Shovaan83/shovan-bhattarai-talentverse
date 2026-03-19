using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Configuration;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Credits;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class CreditService : ICreditService
{
    private readonly ICreditRepository _creditRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IOptions<AppConfigOptions> _appConfig;
    private readonly IOptions<StripeSettings> _stripeSettings;
    private readonly ILogger<CreditService> _logger;

    // Credit pack definitions — in a real app these could be in DB / config
    private static readonly List<CreditPackDto> CreditPacks =
    [
        new CreditPackDto { Id = "pack_100",  Name = "Starter Pack",   Credits = 100,  PriceUsd = 4.99m },
        new CreditPackDto { Id = "pack_300",  Name = "Popular Pack",   Credits = 300,  PriceUsd = 9.99m,  BadgeLabel = "Best Value" },
        new CreditPackDto { Id = "pack_750",  Name = "Power Pack",     Credits = 750,  PriceUsd = 19.99m },
        new CreditPackDto { Id = "pack_2000", Name = "Ultimate Pack",  Credits = 2000, PriceUsd = 49.99m }
    ];

    public CreditService(
        ICreditRepository creditRepository,
        UserManager<AppUser> userManager,
        IOptions<AppConfigOptions> appConfig,
        IOptions<StripeSettings> stripeSettings,
        ILogger<CreditService> logger)
    {
        _creditRepository = creditRepository;
        _userManager = userManager;
        _appConfig = appConfig;
        _stripeSettings = stripeSettings;
        _logger = logger;
    }

    public async Task<ServiceResponse<WalletDto>> GetWalletAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<WalletDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<WalletDto>.FailureResponse(AppConstant.ErrorMessages.UserNotFound);

            var balance = await _creditRepository.GetBalanceAsync(userId);
            var swaps = await _creditRepository.GetCompletedSwapCountAsync(userId);

            // Compute totalEarned and totalSpent from balance history
            var filter = new TransactionFilterDto { Page = 1, PageSize = 10000 };
            var history = await _creditRepository.GetTransactionsAsync(userId, filter);

            decimal totalEarned = history.Transactions
                .Where(t => t.Amount > 0)
                .Sum(t => t.Amount);

            decimal totalSpent = history.Transactions
                .Where(t => t.Amount < 0)
                .Sum(t => Math.Abs(t.Amount));

            var wallet = new WalletDto
            {
                UserId = userId,
                Username = user.UserName ?? string.Empty,
                Balance = balance,
                TotalSwapsCompleted = swaps,
                TotalEarned = totalEarned,
                TotalSpent = totalSpent
            };

            return ServiceResponse<WalletDto>.SuccessResponse(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching wallet for user {UserId}", userId);
            return ServiceResponse<WalletDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<TransactionListResponseDto>> GetTransactionsAsync(string userId, TransactionFilterDto filter)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResponse<TransactionListResponseDto>.FailureResponse(AppConstant.ErrorMessages.UserIdRequired);

            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1 || filter.PageSize > 100) filter.PageSize = 20;

            var result = await _creditRepository.GetTransactionsAsync(userId, filter);
            return ServiceResponse<TransactionListResponseDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions for user {UserId}", userId);
            return ServiceResponse<TransactionListResponseDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<LeaderboardResponseDto>> GetLeaderboardAsync(string currentUserId)
    {
        try
        {
            var entries = await _creditRepository.GetLeaderboardAsync(50);
            var rank = await _creditRepository.GetUserRankAsync(currentUserId);
            var balance = await _creditRepository.GetBalanceAsync(currentUserId);

            var response = new LeaderboardResponseDto
            {
                Entries = entries,
                CurrentUserRank = rank,
                CurrentUserBalance = balance
            };

            return ServiceResponse<LeaderboardResponseDto>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leaderboard for user {UserId}", currentUserId);
            return ServiceResponse<LeaderboardResponseDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public Task<ServiceResponse<IEnumerable<CreditPackDto>>> GetCreditPacksAsync()
    {
        return Task.FromResult(
            ServiceResponse<IEnumerable<CreditPackDto>>.SuccessResponse(CreditPacks));
    }

    public async Task<ServiceResponse<CheckoutSessionDto>> CreateCheckoutSessionAsync(
        string userId, string packId, string successUrl, string cancelUrl)
    {
        try
        {
            var pack = CreditPacks.FirstOrDefault(p => p.Id == packId);
            if (pack == null)
                return ServiceResponse<CheckoutSessionDto>.FailureResponse(AppConstant.ErrorMessages.CreditPackNotFound);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<CheckoutSessionDto>.FailureResponse(AppConstant.ErrorMessages.UserNotFound);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(pack.PriceUsd * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"TalentVerse — {pack.Name}",
                                Description = $"{pack.Credits} credits"
                            }
                        },
                        Quantity = 1
                    }
                ],
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = user.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId },
                    { "packId", packId },
                    { "credits", pack.Credits.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return ServiceResponse<CheckoutSessionDto>.SuccessResponse(
                new CheckoutSessionDto { SessionId = session.Id, Url = session.Url },
                AppConstant.SuccessMessages.CheckoutSessionCreated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe checkout session for user {UserId}", userId);
            return ServiceResponse<CheckoutSessionDto>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<bool>> HandleStripeWebhookAsync(string json, string stripeSignature)
    {
        try
        {
            _logger.LogInformation("⚡ Stripe webhook received — validating signature...");

            var webhookSecret = _stripeSettings.Value.WebhookSecret;

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                _logger.LogError("❌ Stripe WebhookSecret is empty in configuration. Cannot validate webhook.");
                return ServiceResponse<bool>.FailureResponse("Webhook secret not configured.");
            }

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "❌ Invalid Stripe webhook signature. Is the WebhookSecret in appsettings matching the one from 'stripe listen'?");
                return ServiceResponse<bool>.FailureResponse("Invalid webhook signature.");
            }

            _logger.LogInformation("✅ Signature valid — Event type: {EventType}, Event ID: {EventId}",
                stripeEvent.Type, stripeEvent.Id);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session?.Metadata == null)
                {
                    _logger.LogWarning("⚠️ checkout.session.completed received but session metadata is null. Skipping.");
                    return ServiceResponse<bool>.SuccessResponse(true);
                }

                var userId = session.Metadata.GetValueOrDefault("userId");
                var creditsStr = session.Metadata.GetValueOrDefault("credits");
                var sessionId = session.Id;

                _logger.LogInformation(
                    "📦 Checkout session {SessionId} — UserId: {UserId}, Credits: {Credits}",
                    sessionId, userId ?? "(missing)", creditsStr ?? "(missing)");

                if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(creditsStr, out var credits))
                {
                    _logger.LogWarning("⚠️ Missing or invalid metadata (userId={UserId}, credits={Credits}). Skipping fulfillment.", userId, creditsStr);
                    return ServiceResponse<bool>.SuccessResponse(true);
                }

                // Idempotency check — prevent duplicate credit grants on webhook retries
                var alreadyFulfilled = await _creditRepository.HasTransactionByReferenceAsync("Purchase", sessionId);
                if (alreadyFulfilled)
                {
                    _logger.LogInformation("ℹ️ Session {SessionId} already fulfilled. Skipping duplicate.", sessionId);
                    return ServiceResponse<bool>.SuccessResponse(true);
                }

                var balance = await _creditRepository.GetBalanceAsync(userId);
                var newBalance = balance + credits;

                _logger.LogInformation(
                    "💰 Fulfilling: {Credits} credits for user {UserId} — Balance: {OldBalance} → {NewBalance}",
                    credits, userId, balance, newBalance);

                await _creditRepository.UpdateBalanceAsync(userId, newBalance);
                await _creditRepository.AddTransactionAsync(new Data.Entities.CreditTransaction
                {
                    UserId = userId,
                    Type = TransactionType.Purchase,
                    Amount = credits,
                    BalanceAfter = newBalance,
                    Description = $"Purchased {credits} credits via Stripe (session: {sessionId})",
                    TransactionDate = DateTime.UtcNow,
                    ReferenceType = "Purchase"
                });

                _logger.LogInformation("✅ Credits awarded successfully. User {UserId} now has {NewBalance} credits.", userId, newBalance);
            }
            else
            {
                _logger.LogInformation("ℹ️ Ignoring Stripe event type: {EventType}", stripeEvent.Type);
            }

            return ServiceResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing Stripe webhook");
            return ServiceResponse<bool>.FailureResponse(AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task AwardSignupBonusAsync(string userId)
    {
        try
        {
            var amount = _appConfig.Value.InitialCreditBalance;
            var newBalance = (decimal)amount;

            await _creditRepository.UpdateBalanceAsync(userId, newBalance);
            await _creditRepository.AddTransactionAsync(new Data.Entities.CreditTransaction
            {
                UserId = userId,
                Type = TransactionType.SignupBonus,
                Amount = amount,
                BalanceAfter = newBalance,
                Description = $"Welcome bonus — {amount} credits to get you started!",
                TransactionDate = DateTime.UtcNow,
                ReferenceType = "Signup"
            });

            _logger.LogInformation("Awarded signup bonus of {Amount} credits to user {UserId}", amount, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awarding signup bonus to user {UserId}", userId);
        }
    }

    public async Task AwardSwapRewardAsync(string proposerId, string recipientId, long proposalId)
    {
        try
        {
            const decimal rewardAmount = 10m;

            // Award proposer
            var proposerBalance = await _creditRepository.GetBalanceAsync(proposerId);
            var newProposerBalance = proposerBalance + rewardAmount;
            await _creditRepository.UpdateBalanceAsync(proposerId, newProposerBalance);
            await _creditRepository.AddTransactionAsync(new Data.Entities.CreditTransaction
            {
                UserId = proposerId,
                Type = TransactionType.SwapReward,
                Amount = rewardAmount,
                BalanceAfter = newProposerBalance,
                Description = "Earned credits for completing a skill swap",
                TransactionDate = DateTime.UtcNow,
                ReferenceId = proposalId,
                ReferenceType = "Proposal"
            });

            // Award recipient
            var recipientBalance = await _creditRepository.GetBalanceAsync(recipientId);
            var newRecipientBalance = recipientBalance + rewardAmount;
            await _creditRepository.UpdateBalanceAsync(recipientId, newRecipientBalance);
            await _creditRepository.AddTransactionAsync(new Data.Entities.CreditTransaction
            {
                UserId = recipientId,
                Type = TransactionType.SwapReward,
                Amount = rewardAmount,
                BalanceAfter = newRecipientBalance,
                Description = "Earned credits for completing a skill swap",
                TransactionDate = DateTime.UtcNow,
                ReferenceId = proposalId,
                ReferenceType = "Proposal"
            });

            _logger.LogInformation(
                "Awarded swap reward of {Amount} credits to proposer {ProposerId} and recipient {RecipientId} for proposal {ProposalId}",
                rewardAmount, proposerId, recipientId, proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awarding swap reward for proposal {ProposalId}", proposalId);
        }
    }

    public async Task AwardBadgeRewardAsync(string userId, decimal amount, string badgeName)
    {
        try
        {
            if (amount <= 0) return;

            var balance = await _creditRepository.GetBalanceAsync(userId);
            var newBalance = balance + amount;

            await _creditRepository.UpdateBalanceAsync(userId, newBalance);
            await _creditRepository.AddTransactionAsync(new Data.Entities.CreditTransaction
            {
                UserId = userId,
                Type = TransactionType.BadgeReward,
                Amount = amount,
                BalanceAfter = newBalance,
                Description = $"Badge reward for earning \"{badgeName}\"",
                TransactionDate = DateTime.UtcNow,
                ReferenceType = "Badge"
            });

            _logger.LogInformation("Awarded {Amount} credits to user {UserId} for badge {BadgeName}", amount, userId, badgeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awarding badge reward to user {UserId}", userId);
        }
    }

    public Task<decimal> GetBalanceAsync(string userId)
        => _creditRepository.GetBalanceAsync(userId);
}
