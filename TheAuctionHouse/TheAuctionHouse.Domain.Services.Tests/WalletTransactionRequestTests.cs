using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class WalletTransactionRequestTests
    {
        [Fact]
        public void WalletTransactionRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new WalletTransactionRequest();
            Assert.Equal(0, request.UserId);
            Assert.Equal(0, request.Amount);
        }

        [Fact]
        public void WalletTransactionRequest_CanSetProperties()
        {
            var request = new WalletTransactionRequest
            {
                UserId = 42,
                Amount = 1000
            };
            Assert.Equal(42, request.UserId);
            Assert.Equal(1000, request.Amount);
        }
    }
}
