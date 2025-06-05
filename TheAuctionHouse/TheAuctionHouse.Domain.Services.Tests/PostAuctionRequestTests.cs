using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class PostAuctionRequestTests
    {
        [Fact]
        public void PostAuctionRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new PostAuctionRequest();
            Assert.Equal(0, request.AssetId);
            Assert.Equal(0, request.OwnerId);
            Assert.Equal(0, request.ReservedPrice);
            Assert.Equal(0, request.CurrentHighestBid);
            Assert.Equal(0, request.CurrentHighestBidderId);
            Assert.Equal(0, request.MinimumBidIncrement);
            Assert.Equal(0, request.TotalMinutesToExpiry);
        }

        [Fact]
        public void PostAuctionRequest_CanSetProperties()
        {
            var request = new PostAuctionRequest
            {
                AssetId = 10,
                OwnerId = 5,
                ReservedPrice = 1000,
                CurrentHighestBid = 500,
                CurrentHighestBidderId = 2,
                MinimumBidIncrement = 50,
                TotalMinutesToExpiry = 120
            };
            Assert.Equal(10, request.AssetId);
            Assert.Equal(5, request.OwnerId);
            Assert.Equal(1000, request.ReservedPrice);
            Assert.Equal(500, request.CurrentHighestBid);
            Assert.Equal(2, request.CurrentHighestBidderId);
            Assert.Equal(50, request.MinimumBidIncrement);
            Assert.Equal(120, request.TotalMinutesToExpiry);
        }
    }
}
