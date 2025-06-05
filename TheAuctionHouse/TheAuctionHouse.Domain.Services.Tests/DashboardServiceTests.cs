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
using System;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class DashboardServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IAuctionRepository> _mockAuctionRepo = new();
        private readonly Mock<IAssetRepository> _mockAssetRepo = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.AuctionRepository).Returns(_mockAuctionRepo.Object);
            _mockUnitOfWork.Setup(x => x.AssetRepository).Returns(_mockAssetRepo.Object);
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _dashboardService = new DashboardService(_mockUnitOfWork.Object);
        }

        #region GetDashboardDataAsync Tests

        [Fact]
        public void GetDashboardDataAsync_WithValidUserId_ReturnsDashboardData()
        {
            // Arrange
            var userId = 1;
            var liveAuctions = new List<Auction>
            {
                new Auction 
                { 
                    Id = 1, 
                    AssetId = 1, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60,
                    CurrentHighestBid = 1000
                }
            };
            var bidHistories = new List<BidHistory>
            {
                new BidHistory 
                { 
                    Id = 1, 
                    AuctionId = 1, 
                    BidderId = userId, 
                    BidAmount = 900, 
                    BidDate = DateTime.UtcNow.AddMinutes(-15) 
                }
            };
            var asset = new Asset { Id = 1, Title = "Test Asset", Description = "Test Description" };
            var user = new Domain.Entities.PortalUser { Id = userId, Name = "John Doe" };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(liveAuctions);
            _mockAuctionRepo.Setup(x => x.GetBidHistoriesByUserIdAsync(userId)).ReturnsAsync(bidHistories);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = _dashboardService.GetDashboardDataAsync(userId);            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotEmpty(result.Value.LiveAuctions);
            Assert.NotEmpty(result.Value.UserBids);
            Assert.Equal("Test Asset", result.Value.LiveAuctions.First().AssetTitle);
        }

        [Fact]
        public void GetDashboardDataAsync_NoLiveAuctions_ReturnsEmptyLiveAuctions()
        {
            // Arrange
            var userId = 1;
            var liveAuctions = new List<Auction>();
            var bidHistories = new List<BidHistory>();

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(liveAuctions);
            _mockAuctionRepo.Setup(x => x.GetBidHistoriesByUserIdAsync(userId)).ReturnsAsync(bidHistories);

            // Act
            var result = _dashboardService.GetDashboardDataAsync(userId);            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.LiveAuctions);
            Assert.Empty(result.Value.UserBids);
        }

        #endregion

        #region GetLiveAuctionsSortedByExpiryAsync Tests

        [Fact]
        public void GetLiveAuctionsSortedByExpiryAsync_ReturnsAuctionsSortedByExpiry()
        {
            // Arrange
            var auctions = new List<Auction>
            {
                new Auction 
                { 
                    Id = 1, 
                    AssetId = 1, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60 // 30 minutes left
                },
                new Auction 
                { 
                    Id = 2, 
                    AssetId = 2, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-45),
                    TotalMinutesToExpiry = 60 // 15 minutes left - should be first
                },
                new Auction 
                { 
                    Id = 3, 
                    AssetId = 3, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-10),
                    TotalMinutesToExpiry = 60 // 50 minutes left
                }
            };
            var asset1 = new Asset { Id = 1, Title = "Asset 1" };
            var asset2 = new Asset { Id = 2, Title = "Asset 2" };
            var asset3 = new Asset { Id = 3, Title = "Asset 3" };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset1);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(asset2);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(asset3);

            // Act
            var result = _dashboardService.GetLiveAuctionsSortedByExpiryAsync();            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.Count);
            // Should be sorted by expiry (auction 2 should be first as it expires earliest)
            Assert.Equal(2, result.Value.First().AuctionId);
        }

        [Fact]
        public void GetLiveAuctionsSortedByExpiryAsync_FiltersExpiredAuctions()
        {
            // Arrange
            var auctions = new List<Auction>
            {
                new Auction 
                { 
                    Id = 1, 
                    AssetId = 1, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60 // Still live
                },
                new Auction 
                { 
                    Id = 2, 
                    AssetId = 2, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-120),
                    TotalMinutesToExpiry = 60 // Expired
                },
                new Auction 
                { 
                    Id = 3, 
                    AssetId = 3, 
                    Status = AuctionStatus.Expired,
                    StartDate = DateTime.UtcNow.AddMinutes(-120),
                    TotalMinutesToExpiry = 60 // Already marked as expired
                }
            };
            var asset1 = new Asset { Id = 1, Title = "Asset 1" };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset1);

            // Act
            var result = _dashboardService.GetLiveAuctionsSortedByExpiryAsync();            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value); // Only one auction should be returned
            Assert.Equal(1, result.Value.First().AuctionId);
        }

        #endregion

        #region GetUserHighestBidsAsync Tests

        [Fact]
        public void GetUserHighestBidsAsync_ReturnsAuctionsWhereUserHasHighestBid()
        {
            // Arrange
            var userId = 1;
            var auctions = new List<Auction>
            {
                new Auction 
                { 
                    Id = 1, 
                    AssetId = 1, 
                    Status = AuctionStatus.Live, 
                    CurrentHighestBidderId = userId,
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60,
                    CurrentHighestBid = 1000,
                    MinimumBidIncrement = 100
                },
                new Auction 
                { 
                    Id = 2, 
                    AssetId = 2, 
                    Status = AuctionStatus.Live, 
                    CurrentHighestBidderId = 2, // Different user
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60
                },
                new Auction 
                { 
                    Id = 3, 
                    AssetId = 3, 
                    Status = AuctionStatus.Live, 
                    CurrentHighestBidderId = userId,
                    StartDate = DateTime.UtcNow.AddMinutes(-45),
                    TotalMinutesToExpiry = 60 // This expires earlier
                }
            };
            var asset1 = new Asset { Id = 1, Title = "Asset 1", Description = "Description 1" };
            var asset3 = new Asset { Id = 3, Title = "Asset 3", Description = "Description 3" };
            var user = new Domain.Entities.PortalUser { Id = userId, Name = "John Doe" };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset1);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(asset3);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = _dashboardService.GetUserHighestBidsAsync(userId);            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count); // Only auctions where user has highest bid
            // Should be ordered by remaining time (auction 3 should be first as it expires earlier)
            Assert.Equal(3, result.Value.First().AuctionId);
        }

        [Fact]
        public void GetUserHighestBidsAsync_NoHighestBids_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;
            var auctions = new List<Auction>
            {
                new Auction 
                { 
                    Id = 1, 
                    AssetId = 1, 
                    Status = AuctionStatus.Live, 
                    CurrentHighestBidderId = 2, // Different user
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60
                }
            };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);

            // Act
            var result = _dashboardService.GetUserHighestBidsAsync(userId);            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        #endregion
    }
}
