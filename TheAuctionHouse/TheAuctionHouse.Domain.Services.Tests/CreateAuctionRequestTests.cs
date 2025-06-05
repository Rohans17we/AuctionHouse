using System;
using Xunit;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class CreateAuctionRequestTests
    {
        [Fact]
        public void CreateAuctionRequest_DefaultValues_AreSetCorrectly()
        {
            var request = new CreateAuctionRequest();
            Assert.Equal(0, request.AssetId);
            Assert.Equal(0, request.StartingBid);
            Assert.Equal(default(DateTime), request.StartDate);
            Assert.Equal(default(DateTime), request.EndDate);
        }

        [Fact]
        public void CreateAuctionRequest_CanSetProperties()
        {
            var start = new DateTime(2025, 5, 27, 10, 0, 0);
            var end = new DateTime(2025, 5, 28, 10, 0, 0);
            var request = new CreateAuctionRequest
            {
                AssetId = 5,
                StartingBid = 1000,
                StartDate = start,
                EndDate = end
            };
            Assert.Equal(5, request.AssetId);
            Assert.Equal(1000, request.StartingBid);
            Assert.Equal(start, request.StartDate);
            Assert.Equal(end, request.EndDate);
        }
    }
}
