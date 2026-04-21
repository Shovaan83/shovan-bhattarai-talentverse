using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;
using TalentVerse.WebAPI.DTO.Proposals;
using TalentVerse.WebAPI.Interfaces;
using TalentVerse.WebAPI.Services;

namespace TalentVerse.Tests.Services
{
    public class ProposalServiceTests
    {
        #region Test Setup

        private readonly Mock<IProposalRepository> _proposalRepoMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<ILogger<ProposalService>> _loggerMock;
        private readonly Mock<IEmailQueueService> _emailQueueMock;
        private readonly Mock<ICreditService> _creditServiceMock;
        private readonly Mock<IBadgeService> _badgeServiceMock;
        private readonly ProposalService _sut; // System Under Test

        // Common test data
        private const string ProposerId = "proposer-id-123";
        private const string RecipientId = "recipient-id-456";
        private const string UnauthorizedUserId = "unauthorized-id-789";
        private const int ProposerSkillId = 1;
        private const int RecipientSkillId = 2;
        private const int ProposalId = 100;

        public ProposalServiceTests()
        {
            _proposalRepoMock = new Mock<IProposalRepository>();
            _loggerMock = new Mock<ILogger<ProposalService>>();
            _emailQueueMock = new Mock<IEmailQueueService>();
            _creditServiceMock = new Mock<ICreditService>();
            _badgeServiceMock = new Mock<IBadgeService>();

            // UserManager mock setup
            var userStore = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _sut = new ProposalService(
                _proposalRepoMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object,
                _emailQueueMock.Object,
                _creditServiceMock.Object,
                _badgeServiceMock.Object);
        }

        private static AppUser CreateTestUser(string userId, bool isProfileComplete = true)
        {
            return new AppUser
            {
                Id = userId,
                UserName = $"user_{userId}",
                Email = $"{userId}@test.com",
                IsProfileComplete = isProfileComplete
            };
        }

        private static Proposal CreateTestProposalEntity(
            ProposalStatus status = ProposalStatus.Pending,
            bool proposerConfirmed = false,
            bool recipientConfirmed = false)
        {
            return new Proposal
            {
                ProposalId = ProposalId,
                ProposerId = ProposerId,
                RecipientId = RecipientId,
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId,
                Status = status,
                ProposerConfirmed = proposerConfirmed,
                RecipientConfirmed = recipientConfirmed,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
        }

        private static ProposalDto CreateTestProposalDto(
            string status = "Pending",
            bool proposerConfirmed = false,
            bool recipientConfirmed = false)
        {
            return new ProposalDto
            {
                ProposalId = ProposalId,
                ProposerId = ProposerId,
                ProposerUsername = "proposer_user",
                RecipientId = RecipientId,
                RecipientUsername = "recipient_user",
                ProposerUserSkillId = ProposerSkillId,
                ProposerSkillName = "Python",
                ProposerSkillCategory = "Programming",
                RecipientUserSkillId = RecipientSkillId,
                RecipientSkillName = "JavaScript",
                RecipientSkillCategory = "Programming",
                Status = status,
                ProposerConfirmed = proposerConfirmed,
                RecipientConfirmed = recipientConfirmed,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region CreateProposalAsync Tests

        [Fact]
        public async Task CreateProposalAsync_ValidInput_ReturnsSuccessWithProposal()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId,
                Message = "Let's swap skills!"
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            var recipient = CreateTestUser(RecipientId);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(RecipientId))
                .ReturnsAsync(recipient);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((true, (string?)null));

            _proposalRepoMock
                .Setup(x => x.GetUserSkillOwnerAsync(RecipientSkillId))
                .ReturnsAsync(RecipientId);

            _proposalRepoMock
                .Setup(x => x.HasActiveProposalAsync(ProposerId, RecipientId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync(false);

            _proposalRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Proposal>()))
                .ReturnsAsync(CreateTestProposalEntity());

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto());

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.ProposalId.Should().Be(ProposalId);
            result.Message.Should().Be(AppConstant.SuccessMessages.ProposalSent);

            // Verify action flags for proposer
            result.Data.CanCancel.Should().BeTrue();
            result.Data.CanAccept.Should().BeFalse();
            result.Data.CanDecline.Should().BeFalse();
            result.Data.CanConfirmCompletion.Should().BeFalse();
        }

        [Fact]
        public async Task CreateProposalAsync_NullUserId_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            // Act
            var result = await _sut.CreateProposalAsync(null!, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task CreateProposalAsync_EmptyUserId_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            // Act
            var result = await _sut.CreateProposalAsync("   ", dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task CreateProposalAsync_NullDto_ReturnsFailure()
        {
            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, null!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Proposal data is required");
        }

        [Fact]
        public async Task CreateProposalAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task CreateProposalAsync_IncompleteProfile_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: false);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.ProfileNotComplete);
        }

        [Fact]
        public async Task CreateProposalAsync_InvalidSkills_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((false, "You cannot send a proposal to yourself."));

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You cannot send a proposal to yourself.");
        }

        [Fact]
        public async Task CreateProposalAsync_SkillOwnerNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((true, (string?)null));

            _proposalRepoMock
                .Setup(x => x.GetUserSkillOwnerAsync(RecipientSkillId))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Could not determine the skill owner");
        }

        [Fact]
        public async Task CreateProposalAsync_DuplicateActiveProposal_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((true, (string?)null));

            _proposalRepoMock
                .Setup(x => x.GetUserSkillOwnerAsync(RecipientSkillId))
                .ReturnsAsync(RecipientId);

            _proposalRepoMock
                .Setup(x => x.HasActiveProposalAsync(ProposerId, RecipientId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already have an active proposal");
        }

        [Fact]
        public async Task CreateProposalAsync_RepositoryCreateFails_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((true, (string?)null));

            _proposalRepoMock
                .Setup(x => x.GetUserSkillOwnerAsync(RecipientSkillId))
                .ReturnsAsync(RecipientId);

            _proposalRepoMock
                .Setup(x => x.HasActiveProposalAsync(ProposerId, RecipientId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync(false);

            _proposalRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Proposal>()))
                .ReturnsAsync((Proposal?)null);

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to create proposal");
        }

        [Fact]
        public async Task CreateProposalAsync_GetByIdReturnsNull_ReturnsFailure()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((true, (string?)null));

            _proposalRepoMock
                .Setup(x => x.GetUserSkillOwnerAsync(RecipientSkillId))
                .ReturnsAsync(RecipientId);

            _proposalRepoMock
                .Setup(x => x.HasActiveProposalAsync(ProposerId, RecipientId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync(false);

            _proposalRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Proposal>()))
                .ReturnsAsync(CreateTestProposalEntity());

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync((ProposalDto?)null);

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("failed to retrieve details");
        }

        [Fact]
        public async Task CreateProposalAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        [Fact]
        public async Task CreateProposalAsync_QueuesEmailToRecipient()
        {
            // Arrange
            var dto = new CreateProposalDto
            {
                ProposerUserSkillId = ProposerSkillId,
                RecipientUserSkillId = RecipientSkillId,
                Message = "Let's learn together!"
            };

            var proposer = CreateTestUser(ProposerId, isProfileComplete: true);
            var recipient = CreateTestUser(RecipientId);

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);
            _userManagerMock
                .Setup(x => x.FindByIdAsync(RecipientId))
                .ReturnsAsync(recipient);

            _proposalRepoMock
                .Setup(x => x.ValidateSkillsForProposalAsync(ProposerId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync((true, (string?)null));

            _proposalRepoMock
                .Setup(x => x.GetUserSkillOwnerAsync(RecipientSkillId))
                .ReturnsAsync(RecipientId);

            _proposalRepoMock
                .Setup(x => x.HasActiveProposalAsync(ProposerId, RecipientId, ProposerSkillId, RecipientSkillId))
                .ReturnsAsync(false);

            _proposalRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<Proposal>()))
                .ReturnsAsync(CreateTestProposalEntity());

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto());

            // Act
            var result = await _sut.CreateProposalAsync(ProposerId, dto);

            // Assert
            result.Success.Should().BeTrue();
            _emailQueueMock.Verify(x => x.QueueEmailAsync(
                recipient.Email!,
                It.Is<string>(s => s.Contains("Proposal")),
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region GetProposalAsync Tests

        [Fact]
        public async Task GetProposalAsync_ValidProposer_ReturnsProposalWithCorrectFlags()
        {
            // Arrange
            var proposalDto = CreateTestProposalDto("Pending");
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(proposalDto);

            // Act
            var result = await _sut.GetProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.CanCancel.Should().BeTrue();
            result.Data.CanAccept.Should().BeFalse();
            result.Data.CanDecline.Should().BeFalse();
        }

        [Fact]
        public async Task GetProposalAsync_ValidRecipient_ReturnsProposalWithCorrectFlags()
        {
            // Arrange
            var proposalDto = CreateTestProposalDto("Pending");
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(proposalDto);

            // Act
            var result = await _sut.GetProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.CanAccept.Should().BeTrue();
            result.Data.CanDecline.Should().BeTrue();
            result.Data.CanCancel.Should().BeFalse();
        }

        [Fact]
        public async Task GetProposalAsync_AcceptedStatus_ShowsConfirmCompletion()
        {
            // Arrange
            var proposalDto = CreateTestProposalDto("Accepted");
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(proposalDto);

            // Act
            var result = await _sut.GetProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.CanConfirmCompletion.Should().BeTrue();
        }

        [Fact]
        public async Task GetProposalAsync_AlreadyConfirmed_HidesConfirmCompletion()
        {
            // Arrange
            var proposalDto = CreateTestProposalDto("Accepted", proposerConfirmed: true);
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(proposalDto);

            // Act
            var result = await _sut.GetProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.CanConfirmCompletion.Should().BeFalse();
        }

        [Fact]
        public async Task GetProposalAsync_NullUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetProposalAsync(null!, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task GetProposalAsync_InvalidProposalId_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetProposalAsync(ProposerId, 0);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid proposal ID");
        }

        [Fact]
        public async Task GetProposalAsync_NegativeProposalId_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetProposalAsync(ProposerId, -5);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid proposal ID");
        }

        [Fact]
        public async Task GetProposalAsync_ProposalNotFound_ReturnsFailure()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync((ProposalDto?)null);

            // Act
            var result = await _sut.GetProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.ProposalNotFound);
        }

        [Fact]
        public async Task GetProposalAsync_UnauthorizedUser_ReturnsFailure()
        {
            // Arrange
            var proposalDto = CreateTestProposalDto();
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(proposalDto);

            // Act
            var result = await _sut.GetProposalAsync(UnauthorizedUserId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("not authorized to view");
        }

        [Fact]
        public async Task GetProposalAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region GetUserProposalsAsync Tests

        [Fact]
        public async Task GetUserProposalsAsync_ValidRequest_ReturnsPaginatedResults()
        {
            // Arrange
            var filter = new ProposalFilterDto { Page = 1, PageSize = 10 };
            var proposals = new List<ProposalListDto>
            {
                new() { ProposalId = 1, Status = "Pending" },
                new() { ProposalId = 2, Status = "Accepted" }
            };

            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, filter))
                .ReturnsAsync((proposals, 2));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, filter);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Proposals.Should().HaveCount(2);
            result.Data.TotalCount.Should().Be(2);
            result.Data.Page.Should().Be(1);
            result.Data.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetUserProposalsAsync_NullFilter_UsesDefaultValues()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, It.IsAny<ProposalFilterDto>()))
                .ReturnsAsync((new List<ProposalListDto>(), 0));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, null!);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Page.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetUserProposalsAsync_PageLessThanOne_NormalizesToOne()
        {
            // Arrange
            var filter = new ProposalFilterDto { Page = -5, PageSize = 10 };
            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, It.Is<ProposalFilterDto>(f => f.Page == 1)))
                .ReturnsAsync((new List<ProposalListDto>(), 0));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, filter);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Page.Should().Be(1);
        }

        [Fact]
        public async Task GetUserProposalsAsync_PageSizeExceedsMax_ClampedTo50()
        {
            // Arrange
            var filter = new ProposalFilterDto { Page = 1, PageSize = 100 };
            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, It.Is<ProposalFilterDto>(f => f.PageSize == 50)))
                .ReturnsAsync((new List<ProposalListDto>(), 0));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, filter);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.PageSize.Should().Be(50);
        }

        [Fact]
        public async Task GetUserProposalsAsync_PageSizeLessThanOne_ClampedToOne()
        {
            // Arrange
            var filter = new ProposalFilterDto { Page = 1, PageSize = 0 };
            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, It.Is<ProposalFilterDto>(f => f.PageSize == 1)))
                .ReturnsAsync((new List<ProposalListDto>(), 0));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, filter);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.PageSize.Should().Be(1);
        }

        [Fact]
        public async Task GetUserProposalsAsync_NullUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetUserProposalsAsync(null!, new ProposalFilterDto());

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task GetUserProposalsAsync_EmptyUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.GetUserProposalsAsync("", new ProposalFilterDto());

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task GetUserProposalsAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, It.IsAny<ProposalFilterDto>()))
                .ThrowsAsync(new Exception("Query failed"));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, new ProposalFilterDto());

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region AcceptProposalAsync Tests

        [Fact]
        public async Task AcceptProposalAsync_ValidRecipient_ReturnsSuccess()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            var proposer = CreateTestUser(ProposerId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Accepted))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Accepted"));

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be(AppConstant.SuccessMessages.ProposalAccepted);
            result.Data!.Status.Should().Be("Accepted");
        }

        [Fact]
        public async Task AcceptProposalAsync_NullUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.AcceptProposalAsync(null!, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task AcceptProposalAsync_ProposalNotFound_ReturnsFailure()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync((Proposal?)null);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.ProposalNotFound);
        }

        [Fact]
        public async Task AcceptProposalAsync_ProposerTriesToAccept_ReturnsUnauthorized()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only the recipient can accept");
        }

        [Fact]
        public async Task AcceptProposalAsync_UnauthorizedUser_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(UnauthorizedUserId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only the recipient can accept");
        }

        [Fact]
        public async Task AcceptProposalAsync_AlreadyAccepted_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task AcceptProposalAsync_AlreadyRejected_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Rejected);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task AcceptProposalAsync_AlreadyCompleted_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Completed);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task AcceptProposalAsync_AlreadyCancelled_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Cancelled);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task AcceptProposalAsync_UpdateFails_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Accepted))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to accept");
        }

        [Fact]
        public async Task AcceptProposalAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        [Fact]
        public async Task AcceptProposalAsync_QueuesEmailToProposer()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            var proposer = CreateTestUser(ProposerId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Accepted))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Accepted"));

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _emailQueueMock.Verify(x => x.QueueEmailAsync(
                proposer.Email!,
                It.Is<string>(s => s.Contains("Accepted")),
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region DeclineProposalAsync Tests

        [Fact]
        public async Task DeclineProposalAsync_ValidRecipient_ReturnsSuccess()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            var proposer = CreateTestUser(ProposerId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Rejected))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Rejected"));

            _userManagerMock
                .Setup(x => x.FindByIdAsync(ProposerId))
                .ReturnsAsync(proposer);

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be(AppConstant.SuccessMessages.ProposalDeclined);
        }

        [Fact]
        public async Task DeclineProposalAsync_NullUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.DeclineProposalAsync(null!, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task DeclineProposalAsync_ProposalNotFound_ReturnsFailure()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync((Proposal?)null);

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.ProposalNotFound);
        }

        [Fact]
        public async Task DeclineProposalAsync_ProposerTriesToDecline_ReturnsUnauthorized()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.DeclineProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only the recipient can decline");
        }

        [Fact]
        public async Task DeclineProposalAsync_NotPending_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task DeclineProposalAsync_UpdateFails_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Rejected))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to decline");
        }

        [Fact]
        public async Task DeclineProposalAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region CancelProposalAsync Tests

        [Fact]
        public async Task CancelProposalAsync_ValidProposer_ReturnsSuccess()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Cancelled))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Cancelled"));

            _userManagerMock
                .Setup(x => x.FindByIdAsync(RecipientId))
                .ReturnsAsync(recipient);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("cancelled");
        }

        [Fact]
        public async Task CancelProposalAsync_NullUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.CancelProposalAsync(null!, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task CancelProposalAsync_ProposalNotFound_ReturnsFailure()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync((Proposal?)null);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.ProposalNotFound);
        }

        [Fact]
        public async Task CancelProposalAsync_RecipientTriesToCancel_ReturnsUnauthorized()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.CancelProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only the proposer can cancel");
        }

        [Fact]
        public async Task CancelProposalAsync_UnauthorizedUser_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.CancelProposalAsync(UnauthorizedUserId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only the proposer can cancel");
        }

        [Fact]
        public async Task CancelProposalAsync_NotPending_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task CancelProposalAsync_AlreadyCancelled_ReturnsInvalidStateTransition()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Cancelled);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task CancelProposalAsync_UpdateFails_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Cancelled))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to cancel");
        }

        [Fact]
        public async Task CancelProposalAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        [Fact]
        public async Task CancelProposalAsync_QueuesEmailToRecipient()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Cancelled))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Cancelled"));

            _userManagerMock
                .Setup(x => x.FindByIdAsync(RecipientId))
                .ReturnsAsync(recipient);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _emailQueueMock.Verify(x => x.QueueEmailAsync(
                recipient.Email!,
                It.Is<string>(s => s.Contains("Cancelled")),
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ConfirmCompletionAsync Tests

        [Fact]
        public async Task ConfirmCompletionAsync_ProposerFirstConfirmation_ReturnsWaitingMessage()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, proposerConfirmed: false);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, ProposerId, true))
                .ReturnsAsync(true);

            // Updated proposal after confirmation
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Accepted", proposerConfirmed: true));

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Waiting for the other party");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_RecipientFirstConfirmation_ReturnsWaitingMessage()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, recipientConfirmed: false);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, RecipientId, false))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Accepted", recipientConfirmed: true));

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Waiting for the other party");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_BothConfirmed_ReturnsCompletedMessage()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, proposerConfirmed: true);
            var proposer = CreateTestUser(ProposerId);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, RecipientId, false))
                .ReturnsAsync(true);

            // After second confirmation, status becomes Completed
            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Completed", proposerConfirmed: true, recipientConfirmed: true));

            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(proposer);
            _userManagerMock.Setup(x => x.FindByIdAsync(RecipientId)).ReturnsAsync(recipient);

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("Swap completed successfully");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_BothConfirmed_AwardsCredits()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, recipientConfirmed: true);
            var proposer = CreateTestUser(ProposerId);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, ProposerId, true))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Completed", proposerConfirmed: true, recipientConfirmed: true));

            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(proposer);
            _userManagerMock.Setup(x => x.FindByIdAsync(RecipientId)).ReturnsAsync(recipient);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _creditServiceMock.Verify(
                x => x.AwardSwapRewardAsync(ProposerId, RecipientId, ProposalId),
                Times.Once);
        }

        [Fact]
        public async Task ConfirmCompletionAsync_BothConfirmed_EvaluatesBadgesForBoth()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, recipientConfirmed: true);
            var proposer = CreateTestUser(ProposerId);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, ProposerId, true))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Completed", proposerConfirmed: true, recipientConfirmed: true));

            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(proposer);
            _userManagerMock.Setup(x => x.FindByIdAsync(RecipientId)).ReturnsAsync(recipient);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _badgeServiceMock.Verify(x => x.EvaluateOnSwapCompletedAsync(ProposerId), Times.Once);
            _badgeServiceMock.Verify(x => x.EvaluateOnSwapCompletedAsync(RecipientId), Times.Once);
        }

        [Fact]
        public async Task ConfirmCompletionAsync_NullUserId_ReturnsFailure()
        {
            // Act
            var result = await _sut.ConfirmCompletionAsync(null!, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_ProposalNotFound_ReturnsFailure()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync((Proposal?)null);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.ProposalNotFound);
        }

        [Fact]
        public async Task ConfirmCompletionAsync_UnauthorizedUser_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(UnauthorizedUserId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("not authorized to confirm");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_ProposalNotAccepted_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only accepted proposals can be marked as completed");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_RejectedProposal_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Rejected);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only accepted proposals");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_ProposerAlreadyConfirmed_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, proposerConfirmed: true);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already confirmed");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_RecipientAlreadyConfirmed_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, recipientConfirmed: true);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already confirmed");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_UpdateFails_ReturnsFailure()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted);
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, ProposerId, true))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Failed to confirm");
        }

        [Fact]
        public async Task ConfirmCompletionAsync_ExceptionThrown_ReturnsGenericError()
        {
            // Arrange
            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        [Fact]
        public async Task ConfirmCompletionAsync_BothConfirmed_SendsEmailToBothParties()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, proposerConfirmed: true);
            var proposer = CreateTestUser(ProposerId);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock
                .Setup(x => x.GetEntityByIdAsync(ProposalId))
                .ReturnsAsync(proposal);

            _proposalRepoMock
                .Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, RecipientId, false))
                .ReturnsAsync(true);

            _proposalRepoMock
                .Setup(x => x.GetByIdAsync(ProposalId))
                .ReturnsAsync(CreateTestProposalDto("Completed", proposerConfirmed: true, recipientConfirmed: true));

            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(proposer);
            _userManagerMock.Setup(x => x.FindByIdAsync(RecipientId)).ReturnsAsync(recipient);

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _emailQueueMock.Verify(x => x.QueueEmailAsync(
                proposer.Email!,
                It.Is<string>(s => s.Contains("Completed")),
                It.IsAny<string>()), Times.Once);
            _emailQueueMock.Verify(x => x.QueueEmailAsync(
                recipient.Email!,
                It.Is<string>(s => s.Contains("Completed")),
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region State Machine Comprehensive Tests

        [Fact]
        public async Task StateMachine_PendingToAccepted_AllowedByRecipient()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Accepted)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Accepted"));
            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(CreateTestUser(ProposerId));

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task StateMachine_PendingToRejected_AllowedByRecipient()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Rejected)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Rejected"));
            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(CreateTestUser(ProposerId));

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task StateMachine_PendingToCancelled_AllowedByProposer()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Cancelled)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Cancelled"));
            _userManagerMock.Setup(x => x.FindByIdAsync(RecipientId)).ReturnsAsync(CreateTestUser(RecipientId));

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task StateMachine_AcceptedToCompleted_AllowedWhenBothConfirm()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, proposerConfirmed: true);
            var proposer = CreateTestUser(ProposerId);
            var recipient = CreateTestUser(RecipientId);

            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, RecipientId, false)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Completed", true, true));
            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(proposer);
            _userManagerMock.Setup(x => x.FindByIdAsync(RecipientId)).ReturnsAsync(recipient);

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task StateMachine_CompletedIsTerminalState_CannotAccept()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Completed);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task StateMachine_CompletedIsTerminalState_CannotDecline()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Completed);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);

            // Act
            var result = await _sut.DeclineProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task StateMachine_CompletedIsTerminalState_CannotCancel()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Completed);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);

            // Act
            var result = await _sut.CancelProposalAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.InvalidStateTransition);
        }

        [Fact]
        public async Task StateMachine_RejectedIsTerminalState_CannotConfirm()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Rejected);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only accepted proposals");
        }

        [Fact]
        public async Task StateMachine_CancelledIsTerminalState_CannotConfirm()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Cancelled);
            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);

            // Act
            var result = await _sut.ConfirmCompletionAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Only accepted proposals");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task CreateProposalAsync_WhitespaceOnlyUserId_TreatedAsEmpty()
        {
            // Arrange
            var dto = new CreateProposalDto { ProposerUserSkillId = 1, RecipientUserSkillId = 2 };

            // Act
            var result = await _sut.CreateProposalAsync("   \t\n  ", dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("User ID is required");
        }

        [Fact]
        public async Task GetUserProposalsAsync_EmptyResultSet_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var filter = new ProposalFilterDto { Page = 1, PageSize = 10 };
            _proposalRepoMock
                .Setup(x => x.GetUserProposalsAsync(ProposerId, filter))
                .ReturnsAsync((new List<ProposalListDto>(), 0));

            // Act
            var result = await _sut.GetUserProposalsAsync(ProposerId, filter);

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Proposals.Should().BeEmpty();
            result.Data.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task AcceptProposalAsync_ProposerWithNullEmail_DoesNotThrow()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);
            var proposerWithNoEmail = new AppUser { Id = ProposerId, UserName = "no_email_user", Email = null };

            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Accepted)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Accepted"));
            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync(proposerWithNoEmail);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _emailQueueMock.Verify(x => x.QueueEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AcceptProposalAsync_ProposerNotFound_EmailNotSent()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Pending);

            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateStatusAsync(ProposalId, ProposalStatus.Accepted)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Accepted"));
            _userManagerMock.Setup(x => x.FindByIdAsync(ProposerId)).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _sut.AcceptProposalAsync(RecipientId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _emailQueueMock.Verify(x => x.QueueEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmCompletionAsync_NotCompleted_DoesNotAwardCredits()
        {
            // Arrange
            var proposal = CreateTestProposalEntity(ProposalStatus.Accepted, proposerConfirmed: false);

            _proposalRepoMock.Setup(x => x.GetEntityByIdAsync(ProposalId)).ReturnsAsync(proposal);
            _proposalRepoMock.Setup(x => x.UpdateCompletionConfirmationAsync(ProposalId, ProposerId, true)).ReturnsAsync(true);
            _proposalRepoMock.Setup(x => x.GetByIdAsync(ProposalId)).ReturnsAsync(CreateTestProposalDto("Accepted", proposerConfirmed: true));

            // Act
            var result = await _sut.ConfirmCompletionAsync(ProposerId, ProposalId);

            // Assert
            result.Success.Should().BeTrue();
            _creditServiceMock.Verify(x => x.AwardSwapRewardAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
            _badgeServiceMock.Verify(x => x.EvaluateOnSwapCompletedAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
