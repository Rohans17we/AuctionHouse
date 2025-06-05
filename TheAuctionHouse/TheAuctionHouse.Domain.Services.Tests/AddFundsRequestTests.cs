using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AddFundsRequestTests
    {
        [Fact]
        public void AddFundsRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new AddFundsRequest();
            Assert.Equal(0, request.UserId);
            Assert.Equal(0, request.Amount);
        }

        [Fact]
        public void AddFundsRequest_CanSetProperties()
        {
            var request = new AddFundsRequest
            {
                UserId = 42,
                Amount = 1500.75m
            };
            Assert.Equal(42, request.UserId);
            Assert.Equal(1500.75m, request.Amount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(10000)]
        public void AddFundsRequest_Amount_AllowsAnyDecimal(decimal amount)
        {
            var request = new AddFundsRequest
            {
                Amount = amount
            };
            Assert.Equal(amount, request.Amount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999)]
        public void AddFundsRequest_UserId_AllowsAnyInt(int userId)
        {
            var request = new AddFundsRequest
            {
                UserId = userId
            };
            Assert.Equal(userId, request.UserId);
        }
    }
}
