using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using System.Collections.Generic;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class WalletBalanceResponseTests
    {
        [Fact]
        public void WalletBalanceResponse_DefaultValues_AreSetCorrectly()
        {
            var response = new WalletBalanceResponse();
            Assert.Equal(0, response.UserId);
            Assert.Equal(0, response.Amount);
            Assert.Equal(0, response.BlockedAmount);
            Assert.NotNull(response.BidHistory);
            Assert.Empty(response.BidHistory);
        }

        [Fact]
        public void WalletBalanceResponse_CanSetProperties()
        {
            var bidHistory = new List<BidHistoryResponse>
            {
                new BidHistoryResponse { BidId = 1, AuctionId = 2, UserId = 3, BidAmount = 100, UserName = "Alice" }
            };
            var response = new WalletBalanceResponse
            {
                UserId = 10,
                Amount = 5000,
                BlockedAmount = 1000,
                BidHistory = bidHistory
            };
            Assert.Equal(10, response.UserId);
            Assert.Equal(5000, response.Amount);
            Assert.Equal(1000, response.BlockedAmount);
            Assert.Equal(bidHistory, response.BidHistory);
        }
    }
}
