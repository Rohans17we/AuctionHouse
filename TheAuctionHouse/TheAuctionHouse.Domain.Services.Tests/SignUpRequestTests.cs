using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class SignUpRequestTests
    {
        [Fact]
        public void SignUpRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new SignUpRequest();
            Assert.Equal(string.Empty, request.Name);
            Assert.Equal(string.Empty, request.EmailId);
            Assert.Equal(string.Empty, request.Password);
        }

        [Fact]
        public void SignUpRequest_CanSetProperties()
        {
            var request = new SignUpRequest
            {
                Name = "Alice",
                EmailId = "alice@email.com",
                Password = "securepass"
            };
            Assert.Equal("Alice", request.Name);
            Assert.Equal("alice@email.com", request.EmailId);
            Assert.Equal("securepass", request.Password);
        }
    }
}
