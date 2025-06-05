using Xunit;
using Moq;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using TheAuctionHouse.Common.ErrorHandling;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AdministratorServiceUserManagementTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly AdministratorService _adminService;

        public AdministratorServiceUserManagementTests()
        {
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _adminService = new AdministratorService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void ManageUserAccessAsync_WithValidBlockRequest_ReturnsTrue()
        {
            var request = new AdminUserManagementRequest { UserId = 1, Action = "Block" };
            var user = new Domain.Entities.PortalUser { Id = 1 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);
            var result = _adminService.ManageUserAccessAsync(request);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ManageUserAccessAsync_WithValidAllowRequest_ReturnsTrue()
        {
            var request = new AdminUserManagementRequest { UserId = 1, Action = "Allow" };
            var user = new Domain.Entities.PortalUser { Id = 1 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);
            var result = _adminService.ManageUserAccessAsync(request);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ManageUserAccessAsync_UserNotFound_ReturnsError()
        {
            var request = new AdminUserManagementRequest { UserId = 99, Action = "Block" };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(99)).ReturnsAsync((Domain.Entities.PortalUser?)null);
            var result = _adminService.ManageUserAccessAsync(request);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ManageUserAccessAsync_InvalidAction_ReturnsError()
        {
            var request = new AdminUserManagementRequest { UserId = 1, Action = "Suspend" };
            var user = new Domain.Entities.PortalUser { Id = 1 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(user);
            var result = _adminService.ManageUserAccessAsync(request);
            Assert.False(result.IsSuccess);
        }
    }
}
