using TheAuctionHouse.Common.ErrorHandling;
//using TheAuctionHouse.Domain.Entities;

public interface IAssetService
{    Task<Result<bool>> CreateAssetAsync(AssetInformationUpdateRequest createAssetRequest);
    Task<Result<bool>> UpdateAssetAsync(AssetInformationUpdateRequest updateAssetRequest);
    Task<Result<bool>> DeleteAssetAsync(int assetId, int userId);
    Task<Result<AssetResponse>> GetAssetByIdAsync(int assetId);
    Task<Result<List<AssetResponse>>> GetAllAssetsByUserIdAsync(int userId);
    Task<Result<bool>> OpenToAuctionAsync(OpenAssetToAuctionRequest request);
}