using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using TheAuctionHouse.Common;
using TheAuctionHouse.Common.ErrorHandling;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AdministratorServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly AdministratorService _adminService;

        public AdministratorServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _adminService = new AdministratorService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task RegenerateUserPasswordAsync_WithValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new AdminPasswordResetRequest { UserId = 1, NewPassword = "newpassword123" };
            var user = new Domain.Entities.PortalUser { Id = 1, HashedPassword = "oldhash" };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);
            _mockUserRepo.Setup(x => x.UpdateAsync(user)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminService.RegenerateUserPasswordAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RegenerateUserPasswordAsync_UserNotFound_ReturnsError()
        {
            // Arrange
            var request = new AdminPasswordResetRequest { UserId = 99, NewPassword = "newpassword123" };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(99)).ReturnsAsync((Domain.Entities.PortalUser?)null);

            // Act
            var result = await _adminService.RegenerateUserPasswordAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error.ErrorCode);
        }

        [Fact]
        public async Task RegenerateUserPasswordAsync_EmptyPassword_ReturnsError()
        {
            // Arrange
            var request = new AdminPasswordResetRequest { UserId = 1, NewPassword = "" };
            var user = new Domain.Entities.PortalUser { Id = 1 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _adminService.RegenerateUserPasswordAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.ErrorCode);
        }

        [Fact]
        public async Task RegenerateUserPasswordAsync_ShortPassword_ReturnsError()
        {
            // Arrange
            var request = new AdminPasswordResetRequest { UserId = 1, NewPassword = "123" };
            var user = new Domain.Entities.PortalUser { Id = 1 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _adminService.RegenerateUserPasswordAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Error.ErrorCode);
        }
    }
}
