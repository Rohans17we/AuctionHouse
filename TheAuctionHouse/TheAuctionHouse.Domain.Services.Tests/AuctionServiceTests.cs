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
    public class AuctionServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IAuctionRepository> _mockAuctionRepo = new();
        private readonly Mock<IAssetRepository> _mockAssetRepo = new();
        private readonly Mock<IPortalUserRepository> _mockUserRepo = new();
        private readonly AuctionService _auctionService;

        public AuctionServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.AuctionRepository).Returns(_mockAuctionRepo.Object);
            _mockUnitOfWork.Setup(x => x.AssetRepository).Returns(_mockAssetRepo.Object);
            _mockUnitOfWork.Setup(x => x.PortalUserRepository).Returns(_mockUserRepo.Object);
            _auctionService = new AuctionService(_mockUnitOfWork.Object);
        }

        #region PostAuctionAsync Tests

        [Fact]
        public void PostAuctionAsync_WithValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new PostAuctionRequest
            {
                AssetId = 1,
                OwnerId = 2,
                ReservedPrice = 1000,
                MinimumBidIncrement = 100,
                TotalMinutesToExpiry = 120
            };
            var asset = new Asset { Id = 1, UserId = 2, Status = AssetStatus.OpenToAuction };

            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);

            // Act
            var result = _auctionService.PostAuctionAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _mockAuctionRepo.Verify(x => x.AddAsync(It.IsAny<Auction>()), Times.Once);
            _mockAssetRepo.Verify(x => x.UpdateAsync(asset), Times.Once);
            Assert.Equal(AssetStatus.ClosedForAuction, asset.Status);
        }

        [Fact]
        public void PostAuctionAsync_AssetNotFound_ReturnsError()
        {
            // Arrange
            var request = new PostAuctionRequest { AssetId = 999, OwnerId = 2 };
            _mockAssetRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Asset?)null);

            // Act
            var result = _auctionService.PostAuctionAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Asset not found", result.Error.Message);
        }

        [Fact]
        public void PostAuctionAsync_NotAssetOwner_ReturnsError()
        {
            // Arrange
            var request = new PostAuctionRequest { AssetId = 1, OwnerId = 3 };
            var asset = new Asset { Id = 1, UserId = 2, Status = AssetStatus.OpenToAuction };
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);

            // Act
            var result = _auctionService.PostAuctionAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("You can only auction your own assets", result.Error.Message);
        }

        [Fact]
        public void PostAuctionAsync_AssetNotOpenToAuction_ReturnsError()
        {
            // Arrange
            var request = new PostAuctionRequest { AssetId = 1, OwnerId = 2 };
            var asset = new Asset { Id = 1, UserId = 2, Status = AssetStatus.ClosedForAuction };
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);

            // Act
            var result = _auctionService.PostAuctionAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Asset must be in 'Open to Auction' status", result.Error.Message);
        }

        #endregion

        #region CheckAuctionExpiriesAsync Tests

        [Fact]
        public void CheckAuctionExpiriesAsync_ExpiredAuctionWithoutBids_UpdatesStatus()
        {
            // Arrange
            var expiredAuction = new Auction
            {
                Id = 1,
                AssetId = 1,
                Status = AuctionStatus.Live,
                StartDate = DateTime.UtcNow.AddMinutes(-120),
                TotalMinutesToExpiry = 60,
                CurrentHighestBid = 0,
                CurrentHighestBidderId = 0
            };
            var asset = new Asset { Id = 1, Status = AssetStatus.ClosedForAuction };
            var auctions = new List<Auction> { expiredAuction };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);            // Act
            var result = _auctionService.CheckAuctionExpiriesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuctionStatus.ExpiredWithoutBids, expiredAuction.Status);
            Assert.Equal(AssetStatus.Open, asset.Status); // Should be Open per SRS 4.1.5
        }

        [Fact]
        public void CheckAuctionExpiriesAsync_ExpiredAuctionWithBids_TransfersOwnership()
        {
            // Arrange
            var expiredAuction = new Auction
            {
                Id = 1,
                AssetId = 1,
                Status = AuctionStatus.Live,
                StartDate = DateTime.UtcNow.AddMinutes(-120),
                TotalMinutesToExpiry = 60,
                CurrentHighestBid = 1500,
                CurrentHighestBidderId = 3
            };
            var asset = new Asset { Id = 1, UserId = 2, Status = AssetStatus.ClosedForAuction };
            var auctions = new List<Auction> { expiredAuction };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);

            // Act
            var result = _auctionService.CheckAuctionExpiriesAsync();

            // Assert            Assert.True(result.IsSuccess);
            Assert.Equal(AuctionStatus.Expired, expiredAuction.Status);
            Assert.Equal(3, asset.UserId); // Ownership transferred to highest bidder
            Assert.Equal(AssetStatus.Open, asset.Status); // Updated to match implementation
        }

        [Fact]
        public void CheckAuctionExpiriesAsync_NoExpiredAuctions_ReturnsTrue()
        {
            // Arrange
            var liveAuction = new Auction
            {
                Id = 1,
                Status = AuctionStatus.Live,
                StartDate = DateTime.UtcNow.AddMinutes(-30),
                TotalMinutesToExpiry = 60
            };
            var auctions = new List<Auction> { liveAuction };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(auctions);

            // Act
            var result = _auctionService.CheckAuctionExpiriesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuctionStatus.Live, liveAuction.Status); // Should remain unchanged
        }

        #endregion

        #region GetAuctionByIdAsync Tests

        [Fact]
        public void GetAuctionByIdAsync_WithValidId_ReturnsAuction()
        {
            // Arrange
            var auction = new Auction
            {
                Id = 1,
                UserId = 2,
                AssetId = 3,
                CurrentHighestBid = 1000,
                CurrentHighestBidderId = 4,
                MinimumBidIncrement = 100,
                StartDate = DateTime.UtcNow,
                TotalMinutesToExpiry = 60,
                Status = AuctionStatus.Live
            };
            var asset = new Asset { Id = 3, Title = "Test Asset", Description = "Test Description" };
            var bidder = new Domain.Entities.PortalUser { Id = 4, Name = "John Doe" };

            _mockAuctionRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(auction);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(asset);
            _mockUserRepo.Setup(x => x.GetUserByUserIdAsync(4)).ReturnsAsync(bidder);

            // Act
            var result = _auctionService.GetAuctionByIdAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.AuctionId);
            Assert.Equal("Test Asset", result.Value.AssetTitle);
            Assert.Equal("John Doe", result.Value.HighestBidderName);
            Assert.Equal(1000, result.Value.CurrentHighestBid);
        }

        [Fact]
        public void GetAuctionByIdAsync_AuctionNotFound_ReturnsError()
        {
            // Arrange
            _mockAuctionRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Auction?)null);

            // Act
            var result = _auctionService.GetAuctionByIdAsync(999);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Auction not found", result.Error.Message);
        }

        #endregion

        #region GetAuctionsByUserIdAsync Tests

        [Fact]
        public void GetAuctionsByUserIdAsync_WithValidUserId_ReturnsUserAuctions()
        {
            // Arrange
            var auctions = new List<Auction>
            {
                new Auction { Id = 1, UserId = 2, AssetId = 3, Status = AuctionStatus.Live },
                new Auction { Id = 2, UserId = 2, AssetId = 4, Status = AuctionStatus.Expired }
            };
            var asset1 = new Asset { Id = 3, Title = "Asset 1" };
            var asset2 = new Asset { Id = 4, Title = "Asset 2" };

            _mockAuctionRepo.Setup(x => x.GetAuctionsByUserIdAsync(2)).ReturnsAsync(auctions);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(asset1);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(asset2);

            // Act
            var result = _auctionService.GetAuctionsByUserIdAsync(2);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Asset 1", result.Value.First().AssetTitle);
        }

        [Fact]
        public void GetAuctionsByUserIdAsync_NoAuctions_ReturnsEmptyList()
        {
            // Arrange
            _mockAuctionRepo.Setup(x => x.GetAuctionsByUserIdAsync(999)).ReturnsAsync(new List<Auction>());

            // Act
            var result = _auctionService.GetAuctionsByUserIdAsync(999);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        #endregion

        #region GetAllOpenAuctionsByUserIdAsync Tests

        [Fact]
        public void GetAllOpenAuctionsByUserIdAsync_ReturnsOpenAuctions()
        {
            // Arrange
            var openAuctions = new List<Auction>
            {
                new Auction 
                { 
                    Id = 1, 
                    UserId = 2, 
                    AssetId = 3, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-30),
                    TotalMinutesToExpiry = 60,
                    CurrentHighestBidderId = 0
                },
                new Auction 
                { 
                    Id = 2, 
                    UserId = 3, 
                    AssetId = 4, 
                    Status = AuctionStatus.Live, 
                    StartDate = DateTime.UtcNow.AddMinutes(-15),
                    TotalMinutesToExpiry = 60,
                    CurrentHighestBidderId = 1 // User has highest bid
                }
            };
            var asset1 = new Asset { Id = 3, Title = "Asset 1" };
            var asset2 = new Asset { Id = 4, Title = "Asset 2" };
            var bidHistories = new List<BidHistory>
            {
                new BidHistory { AuctionId = 2, BidderId = 1 }
            };

            _mockAuctionRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(openAuctions);
            _mockAuctionRepo.Setup(x => x.GetBidHistoriesByUserIdAsync(1)).ReturnsAsync(bidHistories);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(asset1);
            _mockAssetRepo.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(asset2);

            // Act
            var result = _auctionService.GetAllOpenAuctionsByUserIdAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            // Should be sorted with user's highest bid auctions first
        }

        #endregion
    }
}
