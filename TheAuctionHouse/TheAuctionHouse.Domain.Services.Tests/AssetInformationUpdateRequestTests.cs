using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AssetInformationUpdateRequestTests
    {
        [Fact]
        public void AssetInformationUpdateRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new AssetInformationUpdateRequest();
            Assert.Equal(string.Empty, request.Title);
            Assert.Equal(string.Empty, request.Description);
            Assert.Equal(0, request.RetailPrice);
        }

        [Fact]
        public void AssetInformationUpdateRequest_CanSetProperties()
        {
            var request = new AssetInformationUpdateRequest
            {
                Title = "Test Asset",
                Description = "Test Description",
                RetailPrice = 1234
            };
            Assert.Equal("Test Asset", request.Title);
            Assert.Equal("Test Description", request.Description);
            Assert.Equal(1234, request.RetailPrice);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AssetInformationUpdateRequest_Title_AllowsNullOrWhitespace(string? title)
        {
            var request = new AssetInformationUpdateRequest { Title = title ?? string.Empty };
            Assert.Equal(title ?? string.Empty, request.Title);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AssetInformationUpdateRequest_Description_AllowsNullOrWhitespace(string? desc)
        {
            var request = new AssetInformationUpdateRequest { Description = desc ?? string.Empty };
            Assert.Equal(desc ?? string.Empty, request.Description);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999999)]
        public void AssetInformationUpdateRequest_RetailPrice_AllowsAnyInt(int price)
        {
            var request = new AssetInformationUpdateRequest { RetailPrice = price };
            Assert.Equal(price, request.RetailPrice);
        }
    }
}
