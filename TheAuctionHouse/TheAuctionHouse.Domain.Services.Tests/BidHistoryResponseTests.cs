using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class BidHistoryResponseTests
    {
        [Fact]
        public void BidHistoryResponse_DefaultValues_AreSetCorrectly()
        {
            var response = new BidHistoryResponse();
            Assert.Equal(0, response.BidId);
            Assert.Equal(0, response.AuctionId);
            Assert.Equal(0, response.UserId);
            Assert.Equal(0, response.BidAmount);
            Assert.Equal(default(DateTime), response.BidTime);
            Assert.Equal(string.Empty, response.UserName);
        }

        [Fact]
        public void BidHistoryResponse_CanSetProperties()
        {
            var now = new DateTime(2025, 5, 27, 10, 0, 0);
            var response = new BidHistoryResponse
            {
                BidId = 1,
                AuctionId = 2,
                UserId = 3,
                BidAmount = 500,
                BidTime = now,
                UserName = "Alice"
            };
            Assert.Equal(1, response.BidId);
            Assert.Equal(2, response.AuctionId);
            Assert.Equal(3, response.UserId);
            Assert.Equal(500, response.BidAmount);
            Assert.Equal(now, response.BidTime);
            Assert.Equal("Alice", response.UserName);
        }
    }
}
