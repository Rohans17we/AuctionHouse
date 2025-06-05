using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class ResetPasswordRequestTests
    {
        [Fact]
        public void ResetPasswordRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new ResetPasswordRequest();
            Assert.Equal(0, request.UserId);
            Assert.Equal(string.Empty, request.OldPassword);
            Assert.Equal(string.Empty, request.NewPassword);
            Assert.Equal(string.Empty, request.ConfirmPassword);
        }

        [Fact]
        public void ResetPasswordRequest_CanSetProperties()
        {
            var request = new ResetPasswordRequest
            {
                UserId = 123,
                OldPassword = "oldpass",
                NewPassword = "newpass",
                ConfirmPassword = "newpass"
            };
            Assert.Equal(123, request.UserId);
            Assert.Equal("oldpass", request.OldPassword);
            Assert.Equal("newpass", request.NewPassword);
            Assert.Equal("newpass", request.ConfirmPassword);
        }
    }
}
