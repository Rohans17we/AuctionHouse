using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AdminUserManagementRequestTests
    {
        [Fact]
        public void AdminUserManagementRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new AdminUserManagementRequest();
            Assert.Equal(0, request.UserId);
            Assert.Equal(string.Empty, request.Action);
        }

        [Fact]
        public void AdminUserManagementRequest_CanSetProperties()
        {
            var request = new AdminUserManagementRequest
            {
                UserId = 456,
                Action = "Block"
            };
            Assert.Equal(456, request.UserId);
            Assert.Equal("Block", request.Action);
        }

        [Theory]
        [InlineData("Block")]
        [InlineData("Allow")]
        [InlineData("Delete")]
        [InlineData("")]
        [InlineData(null)]
        public void AdminUserManagementRequest_Action_AllowsAnyString(string? action)
        {
            var request = new AdminUserManagementRequest
            {
                Action = action ?? string.Empty
            };
            Assert.Equal(action ?? string.Empty, request.Action);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999)]
        public void AdminUserManagementRequest_UserId_AllowsAnyInt(int userId)
        {
            var request = new AdminUserManagementRequest
            {
                UserId = userId
            };
            Assert.Equal(userId, request.UserId);
        }

        [Fact]
        public void AdminUserManagementRequest_ValidActions_CanBeSet()
        {
            var blockRequest = new AdminUserManagementRequest { Action = "Block" };
            var allowRequest = new AdminUserManagementRequest { Action = "Allow" };
            var deleteRequest = new AdminUserManagementRequest { Action = "Delete" };
            
            Assert.Equal("Block", blockRequest.Action);
            Assert.Equal("Allow", allowRequest.Action);
            Assert.Equal("Delete", deleteRequest.Action);
        }
    }
}
