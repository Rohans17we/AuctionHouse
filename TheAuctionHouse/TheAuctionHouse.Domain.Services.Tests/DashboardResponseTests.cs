using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using System.Collections.Generic;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class DashboardResponseTests
    {
        [Fact]
        public void DashboardResponse_DefaultValues_AreSetCorrectly()
        {
            var response = new DashboardResponse();
            Assert.NotNull(response.LiveAuctions);
            Assert.Empty(response.LiveAuctions);
            Assert.NotNull(response.UserBids);
            Assert.Empty(response.UserBids);
            Assert.NotNull(response.UserHighestBids);
            Assert.Empty(response.UserHighestBids);
        }

        [Fact]
        public void DashboardResponse_CanSetProperties()
        {
            var liveAuctions = new List<AuctionResponse> { new AuctionResponse { AuctionId = 1 } };
            var userBids = new List<BidHistoryResponse> { new BidHistoryResponse { BidId = 2 } };
            var userHighestBids = new List<AuctionResponse> { new AuctionResponse { AuctionId = 3 } };
            var response = new DashboardResponse
            {
                LiveAuctions = liveAuctions,
                UserBids = userBids,
                UserHighestBids = userHighestBids
            };
            Assert.Equal(liveAuctions, response.LiveAuctions);
            Assert.Equal(userBids, response.UserBids);
            Assert.Equal(userHighestBids, response.UserHighestBids);
        }
    }
}
