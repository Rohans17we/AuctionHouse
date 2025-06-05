using Xunit;
using Moq;
using System.Threading.Tasks;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;
using System.Collections.Generic;

namespace TheAuctionHouse.Domain.Services.Tests
{
    public class AssetServiceTests
    {
        private readonly Mock<IAppUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<IAssetRepository> _mockAssetRepository = new();
        private readonly AssetService _assetService;

        public AssetServiceTests()
        {
            _mockUnitOfWork.Setup(x => x.AssetRepository).Returns(_mockAssetRepository.Object);
            _assetService = new AssetService(_mockUnitOfWork.Object);
        }        [Fact]
        public async Task CreateAssetAsync_WithValidRequest_ReturnsTrue()
        {
            var request = new AssetInformationUpdateRequest { Title = "Valid Title", Description = "Valid Description", RetailPrice = 100, OwnerId = 1 };
            _mockAssetRepository.Setup(x => x.AddAsync(It.IsAny<Asset>())).Returns(Task.CompletedTask);
            var result = await _assetService.CreateAssetAsync(request);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateAssetAsync_WithInvalidTitle_ReturnsError()
        {
            var request = new AssetInformationUpdateRequest { Title = "Bad", Description = "Valid Description", RetailPrice = 100 };
            var result = _assetService.CreateAssetAsync(request);
            Assert.False(result.IsSuccess);
        }

        // UpdateAssetAsync tests are not implemented in AssetService, so skip for now

        [Fact]
        public void DeleteAssetAsync_WithDraftStatus_ReturnsTrue()
        {
            var asset = new Asset { Id = 1, Status = AssetStatus.Draft };
            _mockAssetRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);
            _mockAssetRepository.Setup(x => x.DeleteAsync(asset)).Returns(Task.CompletedTask);
            var result = _assetService.DeleteAssetAsync(1);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void DeleteAssetAsync_WithNonDeletableStatus_ReturnsError()
        {
            var asset = new Asset { Id = 1, Status = AssetStatus.ClosedForAuction };
            _mockAssetRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);
            var result = _assetService.DeleteAssetAsync(1);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void GetAssetByIdAsync_AssetExists_ReturnsAsset()
        {
            var asset = new Asset { Id = 1, Title = "Asset", Description = "Desc", RetailValue = 100, Status = AssetStatus.OpenToAuction };
            _mockAssetRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(asset);
            var result = _assetService.GetAssetByIdAsync(1);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void GetAssetByIdAsync_AssetNotFound_ReturnsError()
        {
            _mockAssetRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Asset?)null);
            var result = _assetService.GetAssetByIdAsync(1);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void GetAllAssetsByUserIdAsync_ReturnsAssetsList()
        {
            var assets = new List<Asset> { new Asset { Id = 1, Title = "A", Description = "D", RetailValue = 100, Status = AssetStatus.OpenToAuction } };
            _mockAssetRepository.Setup(x => x.GetAssetsByUserIdAsync(It.IsAny<int>())).ReturnsAsync(assets);
            var result = _assetService.GetAllAssetsByUserIdAsync();
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        }
    }
}
