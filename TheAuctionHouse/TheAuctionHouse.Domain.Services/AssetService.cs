using System.Text.RegularExpressions;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class AssetService : IAssetService
{
    private readonly IAppUnitOfWork _appUnitOfWork;

    public AssetService(IAppUnitOfWork appUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
    }    public async Task<Result<List<AssetResponse>>> GetAllAssetsByUserIdAsync(int userId)
    {
        try
        {
            var assets = await _appUnitOfWork.AssetRepository.GetAssetsByUserIdAsync(userId);
            
            var response = assets.Select(asset => new AssetResponse
            {
                AssetId = asset.Id,
                OwnerId = asset.UserId,  // Add the owner ID
                Title = asset.Title,
                Description = asset.Description,
                RetailPrice = asset.RetailValue,
                Status = asset.Status.ToString(),
                OwnerName = "" // TODO: Get owner name from user service
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<bool>> CreateAssetAsync(AssetInformationUpdateRequest createAssetRequest)
    {
        if (createAssetRequest == null)
        {
            return Error.BadRequest("Asset creation request cannot be null");
        }

        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(createAssetRequest, validationError))
            {
                return validationError;
            }

            // Additional business rule validations
            var titleValidation = ValidateTitle(createAssetRequest.Title);
            if (!titleValidation.IsSuccess)
                return titleValidation.Error;

            var descriptionValidation = ValidateDescription(createAssetRequest.Description);
            if (!descriptionValidation.IsSuccess)
                return descriptionValidation.Error;

            var retailValueValidation = ValidateRetailValue(createAssetRequest.RetailPrice);
            if (!retailValueValidation.IsSuccess)
                return retailValueValidation.Error;

            // Create new asset            
            var asset = new Asset
            {
                Title = NormalizeTitle(createAssetRequest.Title),
                Description = createAssetRequest.Description.Trim(),
                RetailValue = createAssetRequest.RetailPrice,
                Status = AssetStatus.Draft,
                UserId = createAssetRequest.OwnerId
            };

            try
            {
                await _appUnitOfWork.AssetRepository.AddAsync(asset);
                await _appUnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception dbEx)
            {
                return Error.InternalServerError($"Failed to save asset: {dbEx.Message}");
            }
        }
        catch (Exception ex)
        {
            return Error.InternalServerError($"Error creating asset: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateAssetAsync(AssetInformationUpdateRequest updateAssetRequest)
    {
        if (updateAssetRequest == null)
        {
            return Error.BadRequest("Asset update request cannot be null");
        }

        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(updateAssetRequest, validationError))
            {
                return validationError;
            }

            var titleValidation = ValidateTitle(updateAssetRequest.Title);
            if (!titleValidation.IsSuccess)
                return titleValidation.Error;

            var descriptionValidation = ValidateDescription(updateAssetRequest.Description);
            if (!descriptionValidation.IsSuccess)
                return descriptionValidation.Error;

            var retailValueValidation = ValidateRetailValue(updateAssetRequest.RetailPrice);
            if (!retailValueValidation.IsSuccess)
                return retailValueValidation.Error;

            var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(updateAssetRequest.AssetId);
            if (asset == null)
            {
                return Error.NotFound("Asset not found");
            }

            if (asset.Status != AssetStatus.Draft)
            {
                return Error.BadRequest("Only assets in Draft status can be updated");
            }            if (asset.UserId != updateAssetRequest.OwnerId)
            {
                return Error.BadRequest("You can only update your own assets");
            }

            asset.Title = NormalizeTitle(updateAssetRequest.Title);
            asset.Description = updateAssetRequest.Description.Trim();
            asset.RetailValue = updateAssetRequest.RetailPrice;

            try
            {
                await _appUnitOfWork.AssetRepository.UpdateAsync(asset);
                await _appUnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception dbEx)
            {
                return Error.InternalServerError($"Failed to update asset: {dbEx.Message}");
            }
        }
        catch (Exception ex)
        {
            return Error.InternalServerError($"Error updating asset: {ex.Message}");
        }
    }    public async Task<Result<bool>> DeleteAssetAsync(int assetId, int userId)
    {
        try
        {
            var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(assetId);
            if (asset == null)
                return Error.NotFound("Asset not found");

            // SECURITY: Validate ownership - users can only delete their own assets
            if (asset.UserId != userId)
                return Error.BadRequest("You can only delete your own assets");

            // Check if asset can be deleted (must be in Draft or Open status)
            if (asset.Status != AssetStatus.Draft && asset.Status != AssetStatus.Open)
                return Error.BadRequest("Asset can only be deleted when in Draft or Open status");

            await _appUnitOfWork.AssetRepository.DeleteAsync(asset);
            await _appUnitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<AssetResponse>> GetAssetByIdAsync(int assetId)
    {
        try
        {
            var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(assetId);
            if (asset == null)
                return Error.NotFound("Asset not found");

            var response = new AssetResponse
            {
                AssetId = asset.Id,
                OwnerId = asset.UserId, // Make sure to include owner ID
                Title = asset.Title,
                Description = asset.Description,
                RetailPrice = asset.RetailValue,
                Status = asset.Status.ToString(),
                OwnerName = "" // TODO: Get owner name from user repository
            };

            return response;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    public async Task<Result<bool>> OpenToAuctionAsync(OpenAssetToAuctionRequest request)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(request, validationError))
            {
                return validationError;
            }

            // Get the asset
            var asset = await _appUnitOfWork.AssetRepository.GetByIdAsync(request.AssetId);
            if (asset == null)
                return Error.NotFound("Asset not found");

            // Verify asset is in Draft status
            if (asset.Status != AssetStatus.Draft)
                return Error.BadRequest("Only assets in Draft status can be opened for auction");

            // Update status
            asset.Status = AssetStatus.Open;
            await _appUnitOfWork.AssetRepository.UpdateAsync(asset);
            await _appUnitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            return Error.InternalServerError(ex.Message);
        }
    }

    private Result<bool> ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Error.BadRequest("Title cannot be empty");

        if (title.Length < 10 || title.Length > 150)
            return Error.BadRequest("Title must be between 10 and 150 characters");

        // Check for special characters (only allow alphanumeric and spaces)
        if (!Regex.IsMatch(title, @"^[a-zA-Z0-9\s]+$"))
            return Error.BadRequest("Title should not contain special characters");

        return true;
    }

    private Result<bool> ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Error.BadRequest("Description cannot be empty");

        if (description.Length < 10 || description.Length > 1000)
            return Error.BadRequest("Description must be between 10 and 1000 characters");

        return true;
    }

    private Result<bool> ValidateRetailValue(int retailValue)
    {
        if (retailValue <= 0)
            return Error.BadRequest("Retail value must be a positive integer");

        return true;
    }

    private string NormalizeTitle(string title)
    {
        // Replace multiple consecutive spaces with single space and trim edges
        return Regex.Replace(title.Trim(), @"\s+", " ");
    }
}
