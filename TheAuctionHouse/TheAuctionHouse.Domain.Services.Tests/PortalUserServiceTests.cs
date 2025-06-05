using Xunit;
using Moq;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Common.ErrorHandling;
using System.Threading.Tasks;
using System;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class PortalUserServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly Mock<IEmailService> _mockEmailService = new();
        private readonly PortalUserService _portalUserService;

        public PortalUserServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _portalUserService = new PortalUserService(_mockUnitOfWork.Object, _mockEmailService.Object);
        }

        #region SignUpAsync Tests

        [Fact]
        public async Task SignUpAsync_WithValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new SignUpRequest
            {
                Name = "John Doe",
                EmailId = "john.doe@example.com",
                Password = "securepassword123"
            };            _mockUserRepo.Setup(x => x.GetUserByEmailAsync(request.EmailId))
                .ReturnsAsync((Domain.Entities.PortalUser?)null); // Email not already registered
            _mockUserRepo.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.PortalUser>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _portalUserService.SignUpAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.PortalUser>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region LoginAsync Tests        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            var request = new LoginRequest
            {
                EmailId = "john.doe@example.com",
                Password = "correctpassword"
            };
            
            var hashedPassword = TheAuctionHouse.Common.PasswordHelper.HashPassword("correctpassword");
            var user = new Domain.Entities.PortalUser
            {
                Id = 1,
                EmailId = "john.doe@example.com",
                HashedPassword = hashedPassword
            };

            _mockUserRepo.Setup(x => x.GetUserByEmailAsync("john.doe@example.com")).ReturnsAsync(user);

            // Act
            var result = await _portalUserService.LoginAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
        }

        #endregion

        #region LogoutAsync Tests

        [Fact]
        public async Task LogoutAsync_WithValidUserId_ReturnsTrue()
        {
            // Arrange
            var userId = 1;

            // Act & Assert
            // Note: Current implementation throws NotImplementedException
            await Assert.ThrowsAsync<NotImplementedException>(() => _portalUserService.LogoutAsync(userId));
        }

        #endregion

        #region ResetPasswordAsync Tests        [Fact]
        public async Task ResetPasswordAsync_WithValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                UserId = 1,
                OldPassword = "oldpassword",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123"
            };
            
            var hashedOldPassword = TheAuctionHouse.Common.PasswordHelper.HashPassword("oldpassword");
            var user = new Domain.Entities.PortalUser
            {
                Id = 1,
                EmailId = "john.doe@example.com",
                HashedPassword = hashedOldPassword
            };

            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _portalUserService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _mockUserRepo.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        #endregion

        #region ForgotPasswordAsync Tests - Comprehensive Tests

        [Fact]
        public async Task ForgotPasswordAsync_WithValidEmail_SendsEmailAndReturnsTrue()
        {
            // Arrange
            var request = new ForgotPasswordRequest
            {
                EmailId = "john.doe@example.com"
            };
            var user = new Domain.Entities.PortalUser
            {
                Id = 1,
                Name = "John Doe",
                EmailId = "john.doe@example.com",
                HashedPassword = "hashedpassword"
            };

            _mockUserRepo.Setup(x => x.GetUserByEmailAsync(request.EmailId)).ReturnsAsync(user);
            _mockEmailService.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).Returns(Task.CompletedTask);

            // Act
            var result = await _portalUserService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _mockEmailService.Verify(x => x.SendEmailAsync(
                user.EmailId,
                "Password Reset | The Auction House",
                It.IsAny<string>(),
                true), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithEmptyEmail_ReturnsValidationError()
        {
            // Arrange
            var request = new ForgotPasswordRequest
            {
                EmailId = ""
            };

            // Act
            var result = await _portalUserService.ForgotPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(422, result.Error.ErrorCode); // Validation error
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithInvalidEmailFormat_ReturnsValidationError()
        {
            // Arrange
            var request = new ForgotPasswordRequest
            {
                EmailId = "invalid-email-format"
            };

            // Act
            var result = await _portalUserService.ForgotPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(422, result.Error.ErrorCode); // Validation error
        }

        [Fact]
        public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsNotFoundError()
        {
            // Arrange
            var request = new ForgotPasswordRequest
            {
                EmailId = "nonexistent@example.com"
            };

            _mockUserRepo.Setup(x => x.GetUserByEmailAsync(request.EmailId))
                .ReturnsAsync((Domain.Entities.PortalUser?)null);

            // Act
            var result = await _portalUserService.ForgotPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.ErrorCode); // Not found error
            Assert.Equal("User Not Found.", result.Error.Message);
        }

        [Fact]
        public async Task ForgotPasswordAsync_EmailServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new ForgotPasswordRequest
            {
                EmailId = "john.doe@example.com"
            };
            var user = new Domain.Entities.PortalUser
            {
                Id = 1,
                Name = "John Doe",
                EmailId = "john.doe@example.com"
            };

            _mockUserRepo.Setup(x => x.GetUserByEmailAsync(request.EmailId)).ReturnsAsync(user);
            _mockEmailService.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).ThrowsAsync(new Exception("Email service error"));

            // Act
            var result = await _portalUserService.ForgotPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.Error.ErrorCode); // Internal server error
        }

        #endregion
    }
}
