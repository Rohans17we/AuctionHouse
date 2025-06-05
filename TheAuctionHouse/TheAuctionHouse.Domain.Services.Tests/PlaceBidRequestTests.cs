using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class PlaceBidRequestTests
    {
        [Fact]
        public void PlaceBidRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new PlaceBidRequest();
            Assert.Equal(0, request.AuctionId);
            Assert.Equal(0, request.BidderId);
            Assert.Equal(0, request.BidAmount);
        }

        [Fact]
        public void PlaceBidRequest_CanSetProperties()
        {
            var request = new PlaceBidRequest
            {
                AuctionId = 5,
                BidderId = 42,
                BidAmount = 1000
            };
            Assert.Equal(5, request.AuctionId);
            Assert.Equal(42, request.BidderId);
            Assert.Equal(1000, request.BidAmount);
        }
    }
}
