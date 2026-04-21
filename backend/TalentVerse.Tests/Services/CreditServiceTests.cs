using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Configuration;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Credits;
using TalentVerse.WebAPI.Interfaces;
using TalentVerse.WebAPI.Services;

namespace TalentVerse.Tests.Services
{
    public class CreditServiceTests
    {
        private readonly Mock<ICreditRepository> _mockCreditRepo;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<IOptions<AppConfigOptions>> _mockAppConfig;
        private readonly Mock<IOptions<StripeSettings>> _mockStripeSettings;
        private readonly Mock<ILogger<CreditService>> _mockLogger;
        private readonly CreditService _sut;

        public CreditServiceTests()
        {
            _mockCreditRepo = new Mock<ICreditRepository>();
            _mockUserManager = CreateMockUserManager();
            _mockAppConfig = new Mock<IOptions<AppConfigOptions>>();
            _mockStripeSettings = new Mock<IOptions<StripeSettings>>();
            _mockLogger = new Mock<ILogger<CreditService>>();

            // Default config values
            _mockAppConfig.Setup(c => c.Value).Returns(new AppConfigOptions
            {
                InitialCreditBalance = 50,
                OtpLength = 6,
                OtpExpiryMinutes = 5,
                MaxSkillsPerUser = 20
            });

            _mockStripeSettings.Setup(s => s.Value).Returns(new StripeSettings
            {
                SecretKey = "sk_test_xxx",
                PublishableKey = "pk_test_xxx",
                WebhookSecret = "whsec_test_xxx"
            });

            _sut = new CreditService(
                _mockCreditRepo.Object,
                _mockUserManager.Object,
                _mockAppConfig.Object,
                _mockStripeSettings.Object,
                _mockLogger.Object);
        }

        #region Helper Methods

        private static Mock<UserManager<AppUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<AppUser>>();
            return new Mock<UserManager<AppUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static AppUser CreateTestUser(
            string id = "user-123",
            string email = "test@example.com",
            string userName = "testuser")
        {
            return new AppUser
            {
                Id = id,
                Email = email,
                UserName = userName,
                NormalizedEmail = email.ToUpperInvariant(),
                NormalizedUserName = userName.ToUpperInvariant()
            };
        }

        private static TransactionListResponseDto CreateTransactionListResponse(
            IEnumerable<CreditTransactionDto>? transactions = null,
            int totalCount = 0,
            int page = 1,
            int pageSize = 20)
        {
            return new TransactionListResponseDto
            {
                Transactions = transactions ?? [],
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        private static CreditTransactionDto CreateTransactionDto(
            long transactionId = 1,
            decimal amount = 10m,
            TransactionType type = TransactionType.SwapReward,
            decimal balanceAfter = 60m)
        {
            return new CreditTransactionDto
            {
                TransactionId = transactionId,
                UserId = "user-123",
                Type = type,
                TypeLabel = type.ToString(),
                Amount = amount,
                BalanceAfter = balanceAfter,
                TransactionDate = DateTime.UtcNow,
                Description = "Test transaction"
            };
        }

        private static LeaderboardEntryDto CreateLeaderboardEntry(
            int rank = 1,
            string userId = "user-123",
            string username = "testuser",
            decimal creditBalance = 100m,
            int completedSwaps = 5)
        {
            return new LeaderboardEntryDto
            {
                Rank = rank,
                UserId = userId,
                Username = username,
                CreditBalance = creditBalance,
                CompletedSwaps = completedSwaps,
                BadgeCount = 2
            };
        }

        #endregion

        #region GetWalletAsync - Happy Path

        [Fact]
        public async Task GetWalletAsync_ValidUserId_ReturnsWalletWithCorrectData()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);
            var transactions = new List<CreditTransactionDto>
            {
                CreateTransactionDto(amount: 50m, type: TransactionType.SignupBonus),
                CreateTransactionDto(amount: 10m, type: TransactionType.SwapReward),
                CreateTransactionDto(amount: -5m, type: TransactionType.Debit)
            };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(55m);
            _mockCreditRepo.Setup(r => r.GetCompletedSwapCountAsync(userId)).ReturnsAsync(3);
            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ReturnsAsync(CreateTransactionListResponse(transactions, 3));

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.UserId.Should().Be(userId);
            result.Data.Username.Should().Be(user.UserName);
            result.Data.Balance.Should().Be(55m);
            result.Data.TotalSwapsCompleted.Should().Be(3);
            result.Data.TotalEarned.Should().Be(60m); // 50 + 10
            result.Data.TotalSpent.Should().Be(5m);   // abs(-5)
        }

        [Fact]
        public async Task GetWalletAsync_UserWithNoTransactions_ReturnsZeroTotals()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(0m);
            _mockCreditRepo.Setup(r => r.GetCompletedSwapCountAsync(userId)).ReturnsAsync(0);
            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.TotalEarned.Should().Be(0m);
            result.Data.TotalSpent.Should().Be(0m);
            result.Data.TotalSwapsCompleted.Should().Be(0);
        }

        [Fact]
        public async Task GetWalletAsync_UserWithNullUsername_ReturnsEmptyStringUsername()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);
            user.UserName = null;

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(0m);
            _mockCreditRepo.Setup(r => r.GetCompletedSwapCountAsync(userId)).ReturnsAsync(0);
            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Username.Should().Be(string.Empty);
        }

        #endregion

        #region GetWalletAsync - Guard Clauses and Failures

        [Fact]
        public async Task GetWalletAsync_NullUserId_ReturnsFailure()
        {
            // Arrange
            string? userId = null;

            // Act
            var result = await _sut.GetWalletAsync(userId!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserIdRequired);
        }

        [Fact]
        public async Task GetWalletAsync_EmptyUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "";

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserIdRequired);
        }

        [Fact]
        public async Task GetWalletAsync_WhitespaceUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "   ";

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserIdRequired);
        }

        [Fact]
        public async Task GetWalletAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = "nonexistent-user";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserNotFound);
        }

        [Fact]
        public async Task GetWalletAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region GetTransactionsAsync - Happy Path

        [Fact]
        public async Task GetTransactionsAsync_ValidRequest_ReturnsTransactionList()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = 1, PageSize = 10 };
            var transactions = new List<CreditTransactionDto>
            {
                CreateTransactionDto(1, 50m, TransactionType.SignupBonus),
                CreateTransactionDto(2, 10m, TransactionType.SwapReward)
            };
            var response = CreateTransactionListResponse(transactions, 2, 1, 10);

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, filter))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Transactions.Should().HaveCount(2);
            result.Data.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetTransactionsAsync_PageLessThanOne_DefaultsToOne()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = 0, PageSize = 10 };
            TransactionFilterDto? capturedFilter = null;

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .Callback<string, TransactionFilterDto>((_, f) => capturedFilter = f)
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            capturedFilter.Should().NotBeNull();
            capturedFilter!.Page.Should().Be(1);
        }

        [Fact]
        public async Task GetTransactionsAsync_NegativePage_DefaultsToOne()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = -5, PageSize = 10 };
            TransactionFilterDto? capturedFilter = null;

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .Callback<string, TransactionFilterDto>((_, f) => capturedFilter = f)
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            capturedFilter!.Page.Should().Be(1);
        }

        [Fact]
        public async Task GetTransactionsAsync_PageSizeLessThanOne_DefaultsToTwenty()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = 1, PageSize = 0 };
            TransactionFilterDto? capturedFilter = null;

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .Callback<string, TransactionFilterDto>((_, f) => capturedFilter = f)
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            capturedFilter!.PageSize.Should().Be(20);   
        }

        [Fact]
        public async Task GetTransactionsAsync_PageSizeOverOneHundred_DefaultsToTwenty()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = 1, PageSize = 150 };
            TransactionFilterDto? capturedFilter = null;

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .Callback<string, TransactionFilterDto>((_, f) => capturedFilter = f)
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            capturedFilter!.PageSize.Should().Be(20);
        }

        [Fact]
        public async Task GetTransactionsAsync_ValidPageSize_UsesProvidedValue()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = 1, PageSize = 50 };
            TransactionFilterDto? capturedFilter = null;

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .Callback<string, TransactionFilterDto>((_, f) => capturedFilter = f)
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            capturedFilter!.PageSize.Should().Be(50);
        }

        [Fact]
        public async Task GetTransactionsAsync_BoundaryPageSize_UsesValue()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto { Page = 1, PageSize = 100 };
            TransactionFilterDto? capturedFilter = null;

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .Callback<string, TransactionFilterDto>((_, f) => capturedFilter = f)
                .ReturnsAsync(CreateTransactionListResponse());

            // Act
            await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            capturedFilter!.PageSize.Should().Be(100);
        }

        #endregion

        #region GetTransactionsAsync - Failures

        [Fact]
        public async Task GetTransactionsAsync_NullUserId_ReturnsFailure()
        {
            // Arrange
            string? userId = null;
            var filter = new TransactionFilterDto();

            // Act
            var result = await _sut.GetTransactionsAsync(userId!, filter);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserIdRequired);
        }

        [Fact]
        public async Task GetTransactionsAsync_EmptyUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "";
            var filter = new TransactionFilterDto();

            // Act
            var result = await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserIdRequired);
        }

        [Fact]
        public async Task GetTransactionsAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var userId = "user-123";
            var filter = new TransactionFilterDto();

            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetTransactionsAsync(userId, filter);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region GetLeaderboardAsync - Happy Path

        [Fact]
        public async Task GetLeaderboardAsync_ValidRequest_ReturnsLeaderboardWithUserPosition()
        {
            // Arrange
            var currentUserId = "user-123";
            var entries = new List<LeaderboardEntryDto>
            {
                CreateLeaderboardEntry(1, "user-1", "topuser", 500m, 20),
                CreateLeaderboardEntry(2, "user-2", "seconduser", 400m, 15),
                CreateLeaderboardEntry(3, currentUserId, "testuser", 300m, 10)
            };

            _mockCreditRepo.Setup(r => r.GetLeaderboardAsync(50)).ReturnsAsync(entries);
            _mockCreditRepo.Setup(r => r.GetUserRankAsync(currentUserId)).ReturnsAsync(3);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(currentUserId)).ReturnsAsync(300m);

            // Act
            var result = await _sut.GetLeaderboardAsync(currentUserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Entries.Should().HaveCount(3);
            result.Data.CurrentUserRank.Should().Be(3);
            result.Data.CurrentUserBalance.Should().Be(300m);
        }

        [Fact]
        public async Task GetLeaderboardAsync_UserNotOnLeaderboard_ReturnsNullRank()
        {
            // Arrange
            var currentUserId = "new-user";
            var entries = new List<LeaderboardEntryDto>
            {
                CreateLeaderboardEntry(1, "user-1", "topuser", 500m, 20)
            };

            _mockCreditRepo.Setup(r => r.GetLeaderboardAsync(50)).ReturnsAsync(entries);
            _mockCreditRepo.Setup(r => r.GetUserRankAsync(currentUserId)).ReturnsAsync((int?)null);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(currentUserId)).ReturnsAsync(0m);

            // Act
            var result = await _sut.GetLeaderboardAsync(currentUserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.CurrentUserRank.Should().BeNull();
            result.Data.CurrentUserBalance.Should().Be(0m);
        }

        [Fact]
        public async Task GetLeaderboardAsync_EmptyLeaderboard_ReturnsEmptyEntries()
        {
            // Arrange
            var currentUserId = "user-123";

            _mockCreditRepo.Setup(r => r.GetLeaderboardAsync(50))
                .ReturnsAsync(new List<LeaderboardEntryDto>());
            _mockCreditRepo.Setup(r => r.GetUserRankAsync(currentUserId)).ReturnsAsync((int?)null);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(currentUserId)).ReturnsAsync(50m);

            // Act
            var result = await _sut.GetLeaderboardAsync(currentUserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Entries.Should().BeEmpty();
        }

        #endregion

        #region GetLeaderboardAsync - Failures

        [Fact]
        public async Task GetLeaderboardAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var currentUserId = "user-123";

            _mockCreditRepo.Setup(r => r.GetLeaderboardAsync(50))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetLeaderboardAsync(currentUserId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region GetCreditPacksAsync

        [Fact]
        public async Task GetCreditPacksAsync_ReturnsAllAvailablePacks()
        {
            // Act
            var result = await _sut.GetCreditPacksAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(4);
        }

        [Fact]
        public async Task GetCreditPacksAsync_ContainsStarterPack()
        {
            // Act
            var result = await _sut.GetCreditPacksAsync();

            // Assert
            var starterPack = result.Data!.FirstOrDefault(p => p.Id == "pack_100");
            starterPack.Should().NotBeNull();
            starterPack!.Name.Should().Be("Starter Pack");
            starterPack.Credits.Should().Be(100);
            starterPack.PriceUsd.Should().Be(4.99m);
        }

        [Fact]
        public async Task GetCreditPacksAsync_ContainsPopularPackWithBadge()
        {
            // Act
            var result = await _sut.GetCreditPacksAsync();

            // Assert
            var popularPack = result.Data!.FirstOrDefault(p => p.Id == "pack_300");
            popularPack.Should().NotBeNull();
            popularPack!.Name.Should().Be("Popular Pack");
            popularPack.Credits.Should().Be(300);
            popularPack.PriceUsd.Should().Be(9.99m);
            popularPack.BadgeLabel.Should().Be("Best Value");
        }

        [Fact]
        public async Task GetCreditPacksAsync_ContainsPowerPack()
        {
            // Act
            var result = await _sut.GetCreditPacksAsync();

            // Assert
            var powerPack = result.Data!.FirstOrDefault(p => p.Id == "pack_750");
            powerPack.Should().NotBeNull();
            powerPack!.Credits.Should().Be(750);
            powerPack.PriceUsd.Should().Be(19.99m);
        }

        [Fact]
        public async Task GetCreditPacksAsync_ContainsUltimatePack()
        {
            // Act
            var result = await _sut.GetCreditPacksAsync();

            // Assert
            var ultimatePack = result.Data!.FirstOrDefault(p => p.Id == "pack_2000");
            ultimatePack.Should().NotBeNull();
            ultimatePack!.Credits.Should().Be(2000);
            ultimatePack.PriceUsd.Should().Be(49.99m);
        }

        #endregion

        #region CreateCheckoutSessionAsync - Failures (cannot test happy path without Stripe)

        [Fact]
        public async Task CreateCheckoutSessionAsync_InvalidPackId_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var packId = "invalid_pack";

            // Act
            var result = await _sut.CreateCheckoutSessionAsync(userId, packId, "https://success.url", "https://cancel.url");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.CreditPackNotFound);
        }

        [Fact]
        public async Task CreateCheckoutSessionAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = "nonexistent-user";
            var packId = "pack_100";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.CreateCheckoutSessionAsync(userId, packId, "https://success.url", "https://cancel.url");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.UserNotFound);
        }

        [Fact]
        public async Task CreateCheckoutSessionAsync_StripeThrowsException_ReturnsGenericError()
        {
            // Arrange
            var userId = "user-123";
            var packId = "pack_100";
            var user = CreateTestUser(userId);

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            // Stripe will throw because we don't have real API keys

            // Act
            var result = await _sut.CreateCheckoutSessionAsync(userId, packId, "https://success.url", "https://cancel.url");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region HandleStripeWebhookAsync - Failures (cannot test happy path without real Stripe events)

        [Fact]
        public async Task HandleStripeWebhookAsync_EmptyWebhookSecret_ReturnsFailure()
        {
            // Arrange
            _mockStripeSettings.Setup(s => s.Value).Returns(new StripeSettings
            {
                SecretKey = "sk_test_xxx",
                WebhookSecret = "" // Empty webhook secret
            });

            var sut = new CreditService(
                _mockCreditRepo.Object,
                _mockUserManager.Object,
                _mockAppConfig.Object,
                _mockStripeSettings.Object,
                _mockLogger.Object);

            // Act
            var result = await sut.HandleStripeWebhookAsync("{}", "sig_xxx");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Webhook secret not configured.");
        }

        [Fact]
        public async Task HandleStripeWebhookAsync_WhitespaceWebhookSecret_ReturnsFailure()
        {
            // Arrange
            _mockStripeSettings.Setup(s => s.Value).Returns(new StripeSettings
            {
                SecretKey = "sk_test_xxx",
                WebhookSecret = "   " // Whitespace webhook secret
            });

            var sut = new CreditService(
                _mockCreditRepo.Object,
                _mockUserManager.Object,
                _mockAppConfig.Object,
                _mockStripeSettings.Object,
                _mockLogger.Object);

            // Act
            var result = await sut.HandleStripeWebhookAsync("{}", "sig_xxx");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Webhook secret not configured.");
        }

        [Fact]
        public async Task HandleStripeWebhookAsync_InvalidSignature_ReturnsFailure()
        {
            // Arrange
            var json = "{}";
            var invalidSignature = "invalid_signature";

            // Act
            var result = await _sut.HandleStripeWebhookAsync(json, invalidSignature);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid webhook signature.");
        }

        #endregion

        #region AwardSignupBonusAsync

        [Fact]
        public async Task AwardSignupBonusAsync_ValidUser_UpdatesBalanceAndAddsTransaction()
        {
            // Arrange
            var userId = "user-123";
            var bonusAmount = 50;
            CreditTransaction? capturedTransaction = null;

            _mockAppConfig.Setup(c => c.Value).Returns(new AppConfigOptions
            {
                InitialCreditBalance = bonusAmount
            });

            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, bonusAmount))
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .Callback<CreditTransaction>(t => capturedTransaction = t)
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            var sut = new CreditService(
                _mockCreditRepo.Object,
                _mockUserManager.Object,
                _mockAppConfig.Object,
                _mockStripeSettings.Object,
                _mockLogger.Object);

            // Act
            await sut.AwardSignupBonusAsync(userId);

            // Assert
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(userId, bonusAmount), Times.Once);
            _mockCreditRepo.Verify(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()), Times.Once);
            capturedTransaction.Should().NotBeNull();
            capturedTransaction!.UserId.Should().Be(userId);
            capturedTransaction.Type.Should().Be(TransactionType.SignupBonus);
            capturedTransaction.Amount.Should().Be(bonusAmount);
            capturedTransaction.BalanceAfter.Should().Be(bonusAmount);
            capturedTransaction.ReferenceType.Should().Be("Signup");
        }

        [Fact]
        public async Task AwardSignupBonusAsync_ExceptionThrown_DoesNotCrash()
        {
            // Arrange
            var userId = "user-123";

            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act - should not throw
            var exception = await Record.ExceptionAsync(() => _sut.AwardSignupBonusAsync(userId));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task AwardSignupBonusAsync_UsesConfiguredBonusAmount()
        {
            // Arrange
            var userId = "user-123";
            var customBonus = 100;
            decimal? capturedBalance = null;

            _mockAppConfig.Setup(c => c.Value).Returns(new AppConfigOptions
            {
                InitialCreditBalance = customBonus
            });

            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, It.IsAny<decimal>()))
                .Callback<string, decimal>((_, b) => capturedBalance = b)
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            var sut = new CreditService(
                _mockCreditRepo.Object,
                _mockUserManager.Object,
                _mockAppConfig.Object,
                _mockStripeSettings.Object,
                _mockLogger.Object);

            // Act
            await sut.AwardSignupBonusAsync(userId);

            // Assert
            capturedBalance.Should().Be(customBonus);
        }

        #endregion

        #region AwardSwapRewardAsync

        [Fact]
        public async Task AwardSwapRewardAsync_ValidInput_AwardsBothParties()
        {
            // Arrange
            var proposerId = "proposer-123";
            var recipientId = "recipient-456";
            var proposalId = 1L;
            const decimal rewardAmount = 10m;

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(proposerId)).ReturnsAsync(100m);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(recipientId)).ReturnsAsync(200m);
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardSwapRewardAsync(proposerId, recipientId, proposalId);

            // Assert
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(proposerId, 100m + rewardAmount), Times.Once);
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(recipientId, 200m + rewardAmount), Times.Once);
            _mockCreditRepo.Verify(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()), Times.Exactly(2));
        }

        [Fact]
        public async Task AwardSwapRewardAsync_CreatesCorrectTransactionsForProposer()
        {
            // Arrange
            var proposerId = "proposer-123";
            var recipientId = "recipient-456";
            var proposalId = 1L;
            var proposerTransactions = new List<CreditTransaction>();

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(proposerId)).ReturnsAsync(100m);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(recipientId)).ReturnsAsync(200m);
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.Is<CreditTransaction>(t => t.UserId == proposerId)))
                .Callback<CreditTransaction>(t => proposerTransactions.Add(t))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.Is<CreditTransaction>(t => t.UserId == recipientId)))
                .ReturnsAsync(new CreditTransaction { TransactionId = 2 });

            // Act
            await _sut.AwardSwapRewardAsync(proposerId, recipientId, proposalId);

            // Assert
            proposerTransactions.Should().ContainSingle();
            var tx = proposerTransactions.First();
            tx.Type.Should().Be(TransactionType.SwapReward);
            tx.Amount.Should().Be(10m);
            tx.BalanceAfter.Should().Be(110m);
            tx.ReferenceId.Should().Be(proposalId);
            tx.ReferenceType.Should().Be("Proposal");
        }

        [Fact]
        public async Task AwardSwapRewardAsync_ExceptionThrown_DoesNotCrash()
        {
            // Arrange
            var proposerId = "proposer-123";
            var recipientId = "recipient-456";
            var proposalId = 1L;

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act - should not throw
            var exception = await Record.ExceptionAsync(() =>
                _sut.AwardSwapRewardAsync(proposerId, recipientId, proposalId));

            // Assert
            exception.Should().BeNull();
        }

        #endregion

        #region AwardBadgeRewardAsync

        [Fact]
        public async Task AwardBadgeRewardAsync_ValidInput_UpdatesBalanceAndAddsTransaction()
        {
            // Arrange
            var userId = "user-123";
            var amount = 25m;
            var badgeName = "First Swap";
            CreditTransaction? capturedTransaction = null;

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(100m);
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, 125m)).ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .Callback<CreditTransaction>(t => capturedTransaction = t)
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardBadgeRewardAsync(userId, amount, badgeName);

            // Assert
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(userId, 125m), Times.Once);
            capturedTransaction.Should().NotBeNull();
            capturedTransaction!.Type.Should().Be(TransactionType.BadgeReward);
            capturedTransaction.Amount.Should().Be(amount);
            capturedTransaction.Description.Should().Contain(badgeName);
            capturedTransaction.ReferenceType.Should().Be("Badge");
        }

        [Fact]
        public async Task AwardBadgeRewardAsync_ZeroAmount_DoesNothing()
        {
            // Arrange
            var userId = "user-123";
            var amount = 0m;
            var badgeName = "Test Badge";

            // Act
            await _sut.AwardBadgeRewardAsync(userId, amount, badgeName);

            // Assert
            _mockCreditRepo.Verify(r => r.GetBalanceAsync(It.IsAny<string>()), Times.Never);
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
            _mockCreditRepo.Verify(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()), Times.Never);
        }

        [Fact]
        public async Task AwardBadgeRewardAsync_NegativeAmount_DoesNothing()
        {
            // Arrange
            var userId = "user-123";
            var amount = -10m;
            var badgeName = "Test Badge";

            // Act
            await _sut.AwardBadgeRewardAsync(userId, amount, badgeName);

            // Assert
            _mockCreditRepo.Verify(r => r.GetBalanceAsync(It.IsAny<string>()), Times.Never);
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task AwardBadgeRewardAsync_ExceptionThrown_DoesNotCrash()
        {
            // Arrange
            var userId = "user-123";
            var amount = 25m;
            var badgeName = "Test Badge";

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act - should not throw
            var exception = await Record.ExceptionAsync(() =>
                _sut.AwardBadgeRewardAsync(userId, amount, badgeName));

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public async Task AwardBadgeRewardAsync_SmallAmount_ProcessesCorrectly()
        {
            // Arrange
            var userId = "user-123";
            var amount = 0.01m; // Minimum positive amount
            var badgeName = "Micro Badge";

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(100m);
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, 100.01m)).ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardBadgeRewardAsync(userId, amount, badgeName);

            // Assert
            _mockCreditRepo.Verify(r => r.UpdateBalanceAsync(userId, 100.01m), Times.Once);
        }

        #endregion

        #region GetBalanceAsync

        [Fact]
        public async Task GetBalanceAsync_ValidUser_ReturnsBalance()
        {
            // Arrange
            var userId = "user-123";
            var expectedBalance = 150m;

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(expectedBalance);

            // Act
            var result = await _sut.GetBalanceAsync(userId);

            // Assert
            result.Should().Be(expectedBalance);
            _mockCreditRepo.Verify(r => r.GetBalanceAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetBalanceAsync_UserWithZeroBalance_ReturnsZero()
        {
            // Arrange
            var userId = "user-123";

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(0m);

            // Act
            var result = await _sut.GetBalanceAsync(userId);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public async Task GetBalanceAsync_DelegatesToRepository()
        {
            // Arrange
            var userId = "user-123";

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(75m);

            // Act
            await _sut.GetBalanceAsync(userId);

            // Assert
            _mockCreditRepo.Verify(r => r.GetBalanceAsync(userId), Times.Once);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetWalletAsync_LargeNumberOfTransactions_CalculatesTotalsCorrectly()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);
            var transactions = Enumerable.Range(1, 100)
                .Select(i => CreateTransactionDto(i, i % 2 == 0 ? 10m : -5m))
                .ToList();

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(250m);
            _mockCreditRepo.Setup(r => r.GetCompletedSwapCountAsync(userId)).ReturnsAsync(50);
            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ReturnsAsync(CreateTransactionListResponse(transactions, 100));

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.TotalEarned.Should().Be(500m);  // 50 transactions * 10
            result.Data.TotalSpent.Should().Be(250m);    // 50 transactions * 5
        }

        [Fact]
        public async Task GetWalletAsync_OnlyEarnings_ReturnsZeroSpent()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);
            var transactions = new List<CreditTransactionDto>
            {
                CreateTransactionDto(1, 50m),
                CreateTransactionDto(2, 25m),
                CreateTransactionDto(3, 10m)
            };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(85m);
            _mockCreditRepo.Setup(r => r.GetCompletedSwapCountAsync(userId)).ReturnsAsync(0);
            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ReturnsAsync(CreateTransactionListResponse(transactions, 3));

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Data!.TotalEarned.Should().Be(85m);
            result.Data.TotalSpent.Should().Be(0m);
        }

        [Fact]
        public async Task GetWalletAsync_OnlySpending_ReturnsZeroEarned()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);
            var transactions = new List<CreditTransactionDto>
            {
                CreateTransactionDto(1, -20m),
                CreateTransactionDto(2, -15m),
                CreateTransactionDto(3, -5m)
            };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(10m);
            _mockCreditRepo.Setup(r => r.GetCompletedSwapCountAsync(userId)).ReturnsAsync(0);
            _mockCreditRepo.Setup(r => r.GetTransactionsAsync(userId, It.IsAny<TransactionFilterDto>()))
                .ReturnsAsync(CreateTransactionListResponse(transactions, 3));

            // Act
            var result = await _sut.GetWalletAsync(userId);

            // Assert
            result.Data!.TotalEarned.Should().Be(0m);
            result.Data.TotalSpent.Should().Be(40m);
        }

        [Fact]
        public async Task AwardSwapRewardAsync_SameProposerAndRecipient_StillAwardsTwice()
        {
            // Arrange - edge case: user somehow is both proposer and recipient
            var userId = "user-123";
            var proposalId = 1L;

            _mockCreditRepo.SetupSequence(r => r.GetBalanceAsync(userId))
                .ReturnsAsync(100m)  // First call for proposer
                .ReturnsAsync(110m); // Second call for recipient (after first award)
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardSwapRewardAsync(userId, userId, proposalId);

            // Assert - both calls happen even if same user
            _mockCreditRepo.Verify(r => r.GetBalanceAsync(userId), Times.Exactly(2));
            _mockCreditRepo.Verify(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetLeaderboardAsync_LimitsToFiftyEntries()
        {
            // Arrange
            var currentUserId = "user-123";

            _mockCreditRepo.Setup(r => r.GetLeaderboardAsync(50))
                .ReturnsAsync(new List<LeaderboardEntryDto>());
            _mockCreditRepo.Setup(r => r.GetUserRankAsync(currentUserId)).ReturnsAsync((int?)null);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(currentUserId)).ReturnsAsync(0m);

            // Act
            await _sut.GetLeaderboardAsync(currentUserId);

            // Assert
            _mockCreditRepo.Verify(r => r.GetLeaderboardAsync(50), Times.Once);
        }

        #endregion

        #region Logging Verification

        [Fact]
        public async Task GetWalletAsync_Exception_LogsError()
        {
            // Arrange
            var userId = "user-123";
            var user = CreateTestUser(userId);

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _sut.GetWalletAsync(userId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AwardSignupBonusAsync_Success_LogsInformation()
        {
            // Arrange
            var userId = "user-123";

            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, It.IsAny<decimal>()))
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardSignupBonusAsync(userId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("signup bonus")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AwardSwapRewardAsync_Success_LogsInformation()
        {
            // Arrange
            var proposerId = "proposer-123";
            var recipientId = "recipient-456";
            var proposalId = 1L;

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(It.IsAny<string>())).ReturnsAsync(100m);
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardSwapRewardAsync(proposerId, recipientId, proposalId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("swap reward")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AwardBadgeRewardAsync_Success_LogsInformation()
        {
            // Arrange
            var userId = "user-123";
            var amount = 25m;
            var badgeName = "Test Badge";

            _mockCreditRepo.Setup(r => r.GetBalanceAsync(userId)).ReturnsAsync(100m);
            _mockCreditRepo.Setup(r => r.UpdateBalanceAsync(userId, 125m)).ReturnsAsync(true);
            _mockCreditRepo.Setup(r => r.AddTransactionAsync(It.IsAny<CreditTransaction>()))
                .ReturnsAsync(new CreditTransaction { TransactionId = 1 });

            // Act
            await _sut.AwardBadgeRewardAsync(userId, amount, badgeName);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("badge")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
