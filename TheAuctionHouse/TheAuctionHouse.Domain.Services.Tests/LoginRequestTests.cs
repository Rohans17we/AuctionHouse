using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class LoginRequestTests
    {
        [Fact]
        public void LoginRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new LoginRequest();
            Assert.Equal(string.Empty, request.EmailId);
            Assert.Equal(string.Empty, request.Password);
        }

        [Fact]
        public void LoginRequest_CanSetProperties()
        {
            var request = new LoginRequest
            {
                EmailId = "test@email.com",
                Password = "testpass"
            };
            Assert.Equal("test@email.com", request.EmailId);
            Assert.Equal("testpass", request.Password);
        }
    }
}
