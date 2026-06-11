using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Skills;
using TalentVerse.WebAPI.Interfaces;
using TalentVerse.WebAPI.Services;

namespace TalentVerse.Tests.Services
{
    public class SkillServiceTests
    {
        private readonly Mock<ISkillRepository> _mockSkillRepo;
        private readonly Mock<IBadgeService> _mockBadgeService;
        private readonly Mock<ILogger<SkillService>> _mockLogger;
        private readonly SkillService _sut;

        public SkillServiceTests()
        {
            _mockSkillRepo = new Mock<ISkillRepository>();
            _mockBadgeService = new Mock<IBadgeService>();
            _mockLogger = new Mock<ILogger<SkillService>>();
            _sut = new SkillService(_mockSkillRepo.Object, _mockBadgeService.Object, _mockLogger.Object);
        }

        #region Helper Methods

        private static AddSkillDto CreateValidAddSkillDto(
            string skillName = "Python",
            string category = "Programming",
            int type = 0,
            int proficiencyLevel = 3,
            string? description = "I can help with Python")
        {
            return new AddSkillDto
            {
                SkillName = skillName,
                Category = category,
                Type = type,
                ProficiencyLevel = proficiencyLevel,
                Description = description
            };
        }

        private static SkillDto CreateSkillDto(
            int userSkillId = 1,
            string skillName = "Python",
            string category = "Programming",
            string type = "Offer",
            int proficiencyLevel = 3,
            string? description = "I can help with Python")
        {
            return new SkillDto
            {
                UserSkillId = userSkillId,
                SkillName = skillName,
                Category = category,
                Type = type,
                ProficiencyLevel = proficiencyLevel,
                Description = description
            };
        }

        #endregion

        #region AddSkillAsync - Happy Path

        [Fact]
        public async Task AddSkillAsync_ValidInputWithOfferType_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(type: 0); // Offer type

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Skill added successfully.");
            _mockSkillRepo.Verify(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()), Times.Once);
        }

        [Fact]
        public async Task AddSkillAsync_ValidInputWithWantType_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-456";
            var dto = CreateValidAddSkillDto(type: 1); // Want type

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Skill added successfully.");
        }

        [Fact]
        public async Task AddSkillAsync_ValidInput_TriggerssBadgeEvaluation()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto();

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            _mockBadgeService
                .Setup(b => b.EvaluateOnSkillAddedAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            // Badge evaluation runs async in fire-and-forget, so we just verify the skill was added
            _mockSkillRepo.Verify(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()), Times.Once);
        }

        [Fact]
        public async Task AddSkillAsync_WithNullDescription_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: null);

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_WithEmptyDescription_SetsDescriptionToNull()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: "");
            AddSkillDto? capturedDto = null;

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .Callback<string, AddSkillDto>((_, d) => capturedDto = d)
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            capturedDto.Should().NotBeNull();
            capturedDto!.Description.Should().BeNull();
        }

        [Fact]
        public async Task AddSkillAsync_TrimsSkillNameAndCategory_PassesSanitizedDataToRepository()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "  Python  ", category: "  Programming  ");
            AddSkillDto? capturedDto = null;

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .Callback<string, AddSkillDto>((_, d) => capturedDto = d)
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            capturedDto.Should().NotBeNull();
            capturedDto!.SkillName.Should().Be("Python");
            capturedDto.Category.Should().Be("Programming");
        }

        #endregion

        #region AddSkillAsync - Guard Clauses and Input Validation

        [Fact]
        public async Task AddSkillAsync_NullUserId_ReturnsFailure()
        {
            // Arrange
            string? userId = null;
            var dto = CreateValidAddSkillDto();

            // Act
            var result = await _sut.AddSkillAsync(userId!, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
            _mockSkillRepo.Verify(r => r.AddSkillToUserAsync(It.IsAny<string>(), It.IsAny<AddSkillDto>()), Times.Never);
        }

        [Fact]
        public async Task AddSkillAsync_EmptyUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "";
            var dto = CreateValidAddSkillDto();

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
        }

        [Fact]
        public async Task AddSkillAsync_WhitespaceUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "   ";
            var dto = CreateValidAddSkillDto();

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
        }

        [Fact]
        public async Task AddSkillAsync_NullDto_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            AddSkillDto? dto = null;

            // Act
            var result = await _sut.AddSkillAsync(userId, dto!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill data is required.");
        }

        [Fact]
        public async Task AddSkillAsync_NullSkillName_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: null!);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill name is required.");
        }

        [Fact]
        public async Task AddSkillAsync_EmptySkillName_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "");

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill name is required.");
        }

        [Fact]
        public async Task AddSkillAsync_WhitespaceOnlySkillName_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "   ");

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill name is required.");
        }

        [Fact]
        public async Task AddSkillAsync_SkillNameTooShort_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "A"); // 1 character

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill name must be between 2 and 100 characters.");
        }

        [Fact]
        public async Task AddSkillAsync_SkillNameExactlyMinLength_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "JS"); // exactly 2 characters

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_SkillNameTooLong_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: new string('A', 101)); // 101 characters

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill name must be between 2 and 100 characters.");
        }

        [Fact]
        public async Task AddSkillAsync_SkillNameExactlyMaxLength_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: new string('A', 100)); // exactly 100 characters

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_NullCategory_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: null!);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill category is required.");
        }

        [Fact]
        public async Task AddSkillAsync_EmptyCategory_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: "");

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill category is required.");
        }

        [Fact]
        public async Task AddSkillAsync_WhitespaceOnlyCategory_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: "   ");

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill category is required.");
        }

        [Fact]
        public async Task AddSkillAsync_CategoryTooShort_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: "A"); // 1 character

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill category must be between 2 and 50 characters.");
        }

        [Fact]
        public async Task AddSkillAsync_CategoryExactlyMinLength_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: "IT"); // exactly 2 characters

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_CategoryTooLong_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: new string('A', 51)); // 51 characters

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill category must be between 2 and 50 characters.");
        }

        [Fact]
        public async Task AddSkillAsync_CategoryExactlyMaxLength_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(category: new string('A', 50)); // exactly 50 characters

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_InvalidSkillTypeNegative_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(type: -1);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill type must be 0 (Offered) or 1 (Wanted).");
        }

        [Fact]
        public async Task AddSkillAsync_InvalidSkillTypeGreaterThanOne_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(type: 2);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill type must be 0 (Offered) or 1 (Wanted).");
        }

        [Fact]
        public async Task AddSkillAsync_ProficiencyLevelTooLow_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(proficiencyLevel: 0);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Proficiency level must be between 1 and 5.");
        }

        [Fact]
        public async Task AddSkillAsync_ProficiencyLevelTooHigh_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(proficiencyLevel: 6);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Proficiency level must be between 1 and 5.");
        }

        [Fact]
        public async Task AddSkillAsync_DescriptionTooLong_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: new string('A', 501)); // 501 characters

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Description cannot exceed 500 characters.");
        }

        [Fact]
        public async Task AddSkillAsync_DescriptionExactlyMaxLength_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: new string('A', 500)); // exactly 500 characters

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion

        #region AddSkillAsync - Repository Failures

        [Fact]
        public async Task AddSkillAsync_RepositoryReturnsFalse_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto();

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to add skill. It may already exist or an error occurred.");
        }

        [Fact]
        public async Task AddSkillAsync_RepositoryThrowsException_ReturnsGenericError()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto();

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region GetUserSkillsAsync - Happy Path

        [Fact]
        public async Task GetUserSkillsAsync_ValidUserId_ReturnsSkillsList()
        {
            // Arrange
            var userId = "user-123";
            var expectedSkills = new List<SkillDto>
            {
                CreateSkillDto(userSkillId: 1, skillName: "Python", type: "Offer"),
                CreateSkillDto(userSkillId: 2, skillName: "JavaScript", type: "Want")
            };

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ReturnsAsync(expectedSkills);

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(expectedSkills);
        }

        [Fact]
        public async Task GetUserSkillsAsync_UserHasNoSkills_ReturnsEmptyList()
        {
            // Arrange
            var userId = "user-123";

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ReturnsAsync(Enumerable.Empty<SkillDto>());

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserSkillsAsync_RepositoryReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var userId = "user-123";

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ReturnsAsync((IEnumerable<SkillDto>?)null);

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserSkillsAsync_UserHasSingleSkill_ReturnsSingleItem()
        {
            // Arrange
            var userId = "user-123";
            var expectedSkills = new List<SkillDto>
            {
                CreateSkillDto(userSkillId: 1, skillName: "Python", type: "Offer")
            };

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ReturnsAsync(expectedSkills);

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data!.First().SkillName.Should().Be("Python");
        }

        #endregion

        #region GetUserSkillsAsync - Guard Clauses

        [Fact]
        public async Task GetUserSkillsAsync_NullUserId_ReturnsFailure()
        {
            // Arrange
            string? userId = null;

            // Act
            var result = await _sut.GetUserSkillsAsync(userId!);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
            _mockSkillRepo.Verify(r => r.GetUserSkillsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserSkillsAsync_EmptyUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "";

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
        }

        [Fact]
        public async Task GetUserSkillsAsync_WhitespaceUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "   ";

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
        }

        #endregion

        #region GetUserSkillsAsync - Exception Handling

        [Fact]
        public async Task GetUserSkillsAsync_RepositoryThrowsException_ReturnsGenericError()
        {
            // Arrange
            var userId = "user-123";

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region DeleteSkillAsync - Happy Path

        [Fact]
        public async Task DeleteSkillAsync_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 1;

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Skill deleted successfully.");
            _mockSkillRepo.Verify(r => r.DeleteUserSkillAsync(userId, userSkillId), Times.Once);
        }

        [Fact]
        public async Task DeleteSkillAsync_LargeSkillId_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = int.MaxValue;

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeTrue();
        }

        #endregion

        #region DeleteSkillAsync - Guard Clauses

        [Fact]
        public async Task DeleteSkillAsync_NullUserId_ReturnsFailure()
        {
            // Arrange
            string? userId = null;
            var userSkillId = 1;

            // Act
            var result = await _sut.DeleteSkillAsync(userId!, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
            _mockSkillRepo.Verify(r => r.DeleteUserSkillAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSkillAsync_EmptyUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "";
            var userSkillId = 1;

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
        }

        [Fact]
        public async Task DeleteSkillAsync_WhitespaceUserId_ReturnsFailure()
        {
            // Arrange
            var userId = "   ";
            var userSkillId = 1;

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User ID is required.");
        }

        [Fact]
        public async Task DeleteSkillAsync_ZeroSkillId_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 0;

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid skill ID. ID must be a positive number.");
        }

        [Fact]
        public async Task DeleteSkillAsync_NegativeSkillId_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = -1;

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid skill ID. ID must be a positive number.");
        }

        [Fact]
        public async Task DeleteSkillAsync_MinIntSkillId_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = int.MinValue;

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid skill ID. ID must be a positive number.");
        }

        #endregion

        #region DeleteSkillAsync - Authorization and Not Found

        [Fact]
        public async Task DeleteSkillAsync_SkillNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 999; // Non-existent skill

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill not found or you don't have permission to delete it.");
        }

        [Fact]
        public async Task DeleteSkillAsync_SkillBelongsToDifferentUser_ReturnsFailure()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 1; // Skill belongs to another user

            // Repository returns false when skill doesn't belong to user
            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Skill not found or you don't have permission to delete it.");
        }

        #endregion

        #region DeleteSkillAsync - Exception Handling

        [Fact]
        public async Task DeleteSkillAsync_RepositoryThrowsException_ReturnsGenericError()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 1;

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(AppConstant.ErrorMessages.GenericError);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task AddSkillAsync_SkillNameWithSpecialCharacters_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "C# & .NET Core");

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_SkillNameWithUnicodeCharacters_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(skillName: "日本語"); // Japanese characters

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_DescriptionWithNewlines_ReturnsSuccess()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: "Line 1\nLine 2\nLine 3");

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserSkillsAsync_MixedSkillTypes_ReturnsAllSkills()
        {
            // Arrange
            var userId = "user-123";
            var expectedSkills = new List<SkillDto>
            {
                CreateSkillDto(userSkillId: 1, skillName: "Python", type: "Offer"),
                CreateSkillDto(userSkillId: 2, skillName: "JavaScript", type: "Offer"),
                CreateSkillDto(userSkillId: 3, skillName: "Machine Learning", type: "Want"),
                CreateSkillDto(userSkillId: 4, skillName: "Data Science", type: "Want")
            };

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ReturnsAsync(expectedSkills);

            // Act
            var result = await _sut.GetUserSkillsAsync(userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(4);
            result.Data!.Count(s => s.Type == "Offer").Should().Be(2);
            result.Data!.Count(s => s.Type == "Want").Should().Be(2);
        }

        [Fact]
        public async Task AddSkillAsync_WhitespaceOnlyDescription_SetsDescriptionToNull()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: "   ");
            AddSkillDto? capturedDto = null;

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .Callback<string, AddSkillDto>((_, d) => capturedDto = d)
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            // After trimming "   " becomes "", which is then set to null
            capturedDto.Should().NotBeNull();
            capturedDto!.Description.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSkillAsync_SkillIdOfOne_ReturnsSuccess()
        {
            // Arrange - boundary test for minimum valid ID
            var userId = "user-123";
            var userSkillId = 1;

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task AddSkillAsync_ConcurrentAdditionOfSameSkill_FirstSucceedsSecondFails()
        {
            // Arrange - simulates duplicate skill scenario
            var userId = "user-123";
            var dto1 = CreateValidAddSkillDto(skillName: "Python");
            var dto2 = CreateValidAddSkillDto(skillName: "Python");

            var callCount = 0;
            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1; // First call succeeds, second fails
                });

            // Act
            var result1 = await _sut.AddSkillAsync(userId, dto1);
            var result2 = await _sut.AddSkillAsync(userId, dto2);

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeFalse();
            result2.Message.Should().Contain("already exist");
        }

        [Fact]
        public async Task AddSkillAsync_DescriptionTrimsCorrectly()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto(description: "  Valid description  ");
            AddSkillDto? capturedDto = null;

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .Callback<string, AddSkillDto>((_, d) => capturedDto = d)
                .ReturnsAsync(true);

            // Act
            var result = await _sut.AddSkillAsync(userId, dto);

            // Assert
            result.Success.Should().BeTrue();
            capturedDto.Should().NotBeNull();
            capturedDto!.Description.Should().Be("Valid description");
        }

        #endregion

        #region Logging Verification

        [Fact]
        public async Task AddSkillAsync_Success_LogsInformation()
        {
            // Arrange
            var userId = "user-123";
            var dto = CreateValidAddSkillDto();

            _mockSkillRepo
                .Setup(r => r.AddSkillToUserAsync(userId, It.IsAny<AddSkillDto>()))
                .ReturnsAsync(true);

            // Act
            await _sut.AddSkillAsync(userId, dto);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Skill") && v.ToString()!.Contains("added")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddSkillAsync_NullUserId_LogsWarning()
        {
            // Arrange
            string? userId = null;
            var dto = CreateValidAddSkillDto();

            // Act
            await _sut.AddSkillAsync(userId!, dto);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("null or empty userId")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSkillAsync_Success_LogsInformation()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 1;

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("deleted")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteSkillAsync_NotFound_LogsWarning()
        {
            // Arrange
            var userId = "user-123";
            var userSkillId = 999;

            _mockSkillRepo
                .Setup(r => r.DeleteUserSkillAsync(userId, userSkillId))
                .ReturnsAsync(false);

            // Act
            await _sut.DeleteSkillAsync(userId, userSkillId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found") || v.ToString()!.Contains("unauthorized")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUserSkillsAsync_Exception_LogsError()
        {
            // Arrange
            var userId = "user-123";

            _mockSkillRepo
                .Setup(r => r.GetUserSkillsAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            await _sut.GetUserSkillsAsync(userId);

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

        #endregion
    }
}
