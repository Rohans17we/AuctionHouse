using Xunit;
using Moq;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using TheAuctionHouse.Common.ErrorHandling;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class WalletServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly WalletService _walletService;

        public WalletServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _walletService = new WalletService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void DepositAsync_WithValidRequest_ReturnsTrue()
        {
            var request = new WalletTransactionRequest { UserId = 1, Amount = 100 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(1)).ReturnsAsync(new Domain.Entities.PortalUser { Id = 1 });
            _mockUserRepo.Setup(x => x.DepositWalletBalance(1, 100));
            var result = _walletService.DepositAsync(request);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void DepositAsync_WithZeroAmount_ReturnsError()
        {
            var request = new WalletTransactionRequest { UserId = 1, Amount = 0 };
            var result = _walletService.DepositAsync(request);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void DepositAsync_WithNegativeAmount_ReturnsError()
        {
            var request = new WalletTransactionRequest { UserId = 1, Amount = -50 };
            var result = _walletService.DepositAsync(request);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void DepositAsync_WithAmountExceedingMax_ReturnsError()
        {
            var request = new WalletTransactionRequest { UserId = 1, Amount = 1000000 };
            var result = _walletService.DepositAsync(request);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void DepositAsync_WithNonExistentUser_ReturnsError()
        {
            var request = new WalletTransactionRequest { UserId = 99, Amount = 100 };
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(99)).ReturnsAsync((Domain.Entities.PortalUser?)null);
            var result = _walletService.DepositAsync(request);
            Assert.False(result.IsSuccess);
        }
    }
}
