using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AssetResponseTests
    {
        [Fact]
        public void AssetResponse_DefaultValues_AreSetCorrectly()
        {
            var response = new AssetResponse();
            Assert.Equal(0, response.AssetId);
            Assert.Equal(string.Empty, response.Title);
            Assert.Equal(string.Empty, response.Description);
            Assert.Equal(0, response.RetailPrice);
            Assert.Equal(string.Empty, response.Status);
            Assert.Equal(string.Empty, response.OwnerName);
        }

        [Fact]
        public void AssetResponse_CanSetProperties()
        {
            var response = new AssetResponse
            {
                AssetId = 10,
                Title = "Test Asset",
                Description = "Test Description",
                RetailPrice = 5000,
                Status = "OpenToAuction",
                OwnerName = "John Doe"
            };
            Assert.Equal(10, response.AssetId);
            Assert.Equal("Test Asset", response.Title);
            Assert.Equal("Test Description", response.Description);
            Assert.Equal(5000, response.RetailPrice);
            Assert.Equal("OpenToAuction", response.Status);
            Assert.Equal("John Doe", response.OwnerName);
        }
    }
}
