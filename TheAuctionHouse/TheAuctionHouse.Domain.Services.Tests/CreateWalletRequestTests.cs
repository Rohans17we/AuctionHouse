using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class CreateWalletRequestTests
    {
        [Fact]
        public void CreateWalletRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new CreateWalletRequest();
            Assert.Equal(0, request.UserId);
            Assert.Equal(0, request.InitialBalance);
        }

        [Fact]
        public void CreateWalletRequest_CanSetProperties()
        {
            var request = new CreateWalletRequest
            {
                UserId = 42,
                InitialBalance = 1000
            };
            Assert.Equal(42, request.UserId);
            Assert.Equal(1000, request.InitialBalance);
        }
    }
}
