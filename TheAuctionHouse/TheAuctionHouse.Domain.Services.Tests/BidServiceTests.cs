using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Common.ErrorHandling;
using System.Collections.Generic;
using System.Linq;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class BidServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IAuctionRepository> _mockAuctionRepo = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly BidService _bidService;

        public BidServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.AuctionRepository).Returns(_mockAuctionRepo.Object);
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            _bidService = new BidService(_mockUnitOfWork.Object);
        }

        #region PlaceBidAsync Tests

        [Fact]
        public async Task PlaceBidAsync_WithValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new PlaceBidRequest { AuctionId = 1, BidderId = 2, BidAmount = 1000 };
            var auction = new Auction { Id = 1, UserId = 3, Status = AuctionStatus.Live, 
                ReservedPrice = 500, CurrentHighestBid = 0, TotalMinutesToExpiry = 60, StartDate = DateTime.UtcNow.AddMinutes(-30) };
            var bidder = new PortalUser { Id = 2, WalletBalance = 2000, WalletBalanceBlocked = 0 };

            _mockAuctionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(auction);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(2)).ReturnsAsync(bidder);
            _mockUserRepo.Setup(x => x.UpdateAsync(It.IsAny<PortalUser>())).Returns(Task.CompletedTask);
            _mockAuctionRepo.Setup(x => x.UpdateAsync(It.IsAny<Auction>())).Returns(Task.CompletedTask);
            
            // Act
            var result = await _bidService.PlaceBidAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            _mockAuctionRepo.Verify(x => x.UpdateAsync(auction), Times.Once);
            _mockUserRepo.Verify(x => x.UpdateAsync(bidder), Times.Once);
        }

        [Fact]
        public async Task PlaceBidAsync_AuctionNotFound_ReturnsError()
        {
            // Arrange
            var request = new PlaceBidRequest { AuctionId = 999, BidderId = 2, BidAmount = 1000 };
            _mockAuctionRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Auction?)null);

            // Act
            var result = await _bidService.PlaceBidAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal("Auction not found", result.Error.Message);
        }

        [Fact]
        public async Task PlaceBidAsync_BidderNotFound_ReturnsError()
        {
            // Arrange
            var request = new PlaceBidRequest { AuctionId = 1, BidderId = 999, BidAmount = 1000 };
            var auction = new Auction { Id = 1, Status = AuctionStatus.Live, StartDate = DateTime.UtcNow.AddMinutes(-30), TotalMinutesToExpiry = 60 };
            _mockAuctionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(auction);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(999)).ReturnsAsync((PortalUser?)null);

            // Act
            var result = await _bidService.PlaceBidAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal("Bidder not found", result.Error.Message);
        }

        [Fact]
        public async Task PlaceBidAsync_AuctionExpired_ReturnsError()
        {
            // Arrange
            var request = new PlaceBidRequest { AuctionId = 1, BidderId = 2, BidAmount = 1000 };
            var auction = new Auction { Id = 1, Status = AuctionStatus.Live, StartDate = DateTime.UtcNow.AddMinutes(-120), TotalMinutesToExpiry = 60 };
            _mockAuctionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(auction);

            // Act
            var result = await _bidService.PlaceBidAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal("Auction is not active or has expired", result.Error.Message);
        }

        [Fact]
        public async Task PlaceBidAsync_InsufficientBalance_ReturnsError()
        {
            // Arrange
            var request = new PlaceBidRequest { AuctionId = 1, BidderId = 2, BidAmount = 2000 };
            var auction = new Auction { Id = 1, UserId = 3, Status = AuctionStatus.Live, 
                ReservedPrice = 500, CurrentHighestBid = 0, StartDate = DateTime.UtcNow.AddMinutes(-30), TotalMinutesToExpiry = 60 };
            var bidder = new PortalUser { Id = 2, WalletBalance = 1000, WalletBalanceBlocked = 0 };

            _mockAuctionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(auction);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(2)).ReturnsAsync(bidder);

            // Act
            var result = await _bidService.PlaceBidAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal("Insufficient wallet balance to place this bid", result.Error.Message);
        }

        [Fact]
        public async Task GetBidHistoryByAuctionIdAsync_ReturnsEmptyListWhenNoHistory()
        {
            // Arrange
            _mockAuctionRepo.Setup(x => x.GetBidHistoriesByAuctionIdAsync(1))
                .ReturnsAsync(new List<BidHistory>());

            // Act
            var result = await _bidService.GetBidHistoryByAuctionIdAsync(1);

            // Assert            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }
        
        [Fact]
        public async Task CheckAndUnblockPreviousBidsAsync_WithValidData_UnblocksAmounts()
        {
            // Arrange
            var auction = new Auction { Id = 1, CurrentHighestBidderId = 4 };
            var bidHistories = new List<BidHistory> {
                new BidHistory { AuctionId = 1, BidderId = 2, BidAmount = 1000 },
                new BidHistory { AuctionId = 1, BidderId = 3, BidAmount = 800 },
                new BidHistory { AuctionId = 1, BidderId = 4, BidAmount = 1200 }
            };

            var user1 = new PortalUser { Id = 2, WalletBalanceBlocked = 1000 };
            var user2 = new PortalUser { Id = 3, WalletBalanceBlocked = 800 };
            var user3 = new PortalUser { Id = 4, WalletBalanceBlocked = 1200 };

            _mockAuctionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(auction);
            _mockAuctionRepo.Setup(x => x.GetBidHistoriesByAuctionIdAsync(1)).ReturnsAsync(bidHistories);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(2)).ReturnsAsync(user1);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(3)).ReturnsAsync(user2);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(4)).ReturnsAsync(user3);
            _mockUserRepo.Setup(x => x.UpdateAsync(It.IsAny<PortalUser>())).Returns(Task.CompletedTask);

            // Act
            var result = await _bidService.CheckAndUnblockPreviousBidsAsync(1, 4);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(0, user1.WalletBalanceBlocked); // Should be unblocked
            Assert.Equal(0, user2.WalletBalanceBlocked); // Should be unblocked
            Assert.Equal(1200, user3.WalletBalanceBlocked); // Should remain blocked (highest bidder)
        }
        #endregion
    }
}
