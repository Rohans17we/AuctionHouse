using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AdminPasswordResetRequestTests
    {
        [Fact]
        public void AdminPasswordResetRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new AdminPasswordResetRequest();
            Assert.Equal(0, request.UserId);
            Assert.Equal(string.Empty, request.NewPassword);
        }

        [Fact]
        public void AdminPasswordResetRequest_CanSetProperties()
        {
            var request = new AdminPasswordResetRequest
            {
                UserId = 123,
                NewPassword = "newpassword123"
            };
            Assert.Equal(123, request.UserId);
            Assert.Equal("newpassword123", request.NewPassword);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("short")]
        [InlineData("verylongpasswordthatexceedsnormallimits")]
        public void AdminPasswordResetRequest_NewPassword_AllowsAnyString(string? password)
        {
            var request = new AdminPasswordResetRequest
            {
                NewPassword = password ?? string.Empty
            };
            Assert.Equal(password ?? string.Empty, request.NewPassword);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999)]
        public void AdminPasswordResetRequest_UserId_AllowsAnyInt(int userId)
        {
            var request = new AdminPasswordResetRequest
            {
                UserId = userId
            };
            Assert.Equal(userId, request.UserId);
        }
    }
}
