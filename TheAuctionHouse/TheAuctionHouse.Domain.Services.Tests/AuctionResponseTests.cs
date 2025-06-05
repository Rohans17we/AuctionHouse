using System;
using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AuctionResponseTests
    {
        [Fact]
        public void AuctionResponse_DefaultValues_AreSetCorrectly()
        {
            var response = new AuctionResponse();
            Assert.Equal(0, response.AuctionId);
            Assert.Equal(0, response.UserId);
            Assert.Equal(0, response.AssetId);
            Assert.Equal(string.Empty, response.AssetTitle);
            Assert.Equal(string.Empty, response.AssetDescription);
            Assert.Equal(0, response.CurrentHighestBid);
            Assert.Equal(0, response.CurrentHighestBidderId);
            Assert.Equal(string.Empty, response.HighestBidderName);
            Assert.Equal(0, response.MinimumBidIncrement);
            Assert.Equal(0, response.CallFor);
            Assert.Equal(default(DateTime), response.StartDate);
            Assert.Equal(0, response.TotalMinutesToExpiry);
            Assert.Equal(string.Empty, response.Status);
        }

        [Fact]
        public void AuctionResponse_CanSetProperties()
        {
            var response = new AuctionResponse
            {
                AuctionId = 1,
                UserId = 2,
                AssetId = 3,
                AssetTitle = "Test Asset",
                AssetDescription = "Test Desc",
                CurrentHighestBid = 100,
                CurrentHighestBidderId = 4,
                HighestBidderName = "Alice",
                MinimumBidIncrement = 10,
                StartDate = new DateTime(2025, 1, 1),
                TotalMinutesToExpiry = 1440,
                Status = "Live"
            };
            Assert.Equal(1, response.AuctionId);
            Assert.Equal(2, response.UserId);
            Assert.Equal(3, response.AssetId);
            Assert.Equal("Test Asset", response.AssetTitle);
            Assert.Equal("Test Desc", response.AssetDescription);
            Assert.Equal(100, response.CurrentHighestBid);
            Assert.Equal(4, response.CurrentHighestBidderId);
            Assert.Equal("Alice", response.HighestBidderName);
            Assert.Equal(10, response.MinimumBidIncrement);
            Assert.Equal(110, response.CallFor);
            Assert.Equal(new DateTime(2025, 1, 1), response.StartDate);
            Assert.Equal(1440, response.TotalMinutesToExpiry);
            Assert.Equal("Live", response.Status);
        }
    }
}
