using Microsoft.AspNetCore.Mvc;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.WebAPI.Controllers
{
    /// <summary>
    /// Asset management controller for handling asset-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetController(IAssetService assetService)
        {
            _assetService = assetService;
        }        /// <summary>
        /// Creates a new asset
        /// </summary>
        /// <param name="request">Asset creation request containing title, description, and retail price</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsset([FromBody] AssetInformationUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Asset creation request cannot be null");
            }

            var result = await _assetService.CreateAssetAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Asset created successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Updates an existing asset
        /// </summary>
        /// <param name="request">Asset update request containing updated asset information</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsset([FromBody] AssetInformationUpdateRequest request)
        {
            if (request == null)
            {
                return BadRequest("Asset update request cannot be null");
            }

            var result = await _assetService.UpdateAssetAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Asset updated successfully" });
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Deletes an asset by ID
        /// </summary>
        /// <param name="assetId">The ID of the asset to delete</param>
        /// <param name="userId">The ID of the user requesting the deletion</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpDelete("{assetId:int}")]
        public async Task<IActionResult> DeleteAsset(int assetId, [FromQuery] int userId)
        {
            if (assetId <= 0)
            {
                return BadRequest("Asset ID must be a positive integer");
            }

            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = await _assetService.DeleteAssetAsync(assetId, userId);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Asset deleted successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets an asset by ID
        /// </summary>
        /// <param name="assetId">The ID of the asset to retrieve</param>
        /// <returns>Asset details</returns>
        [HttpGet("{assetId:int}")]
        public async Task<IActionResult> GetAssetById(int assetId)
        {
            if (assetId <= 0)
            {
                return BadRequest("Asset ID must be a positive integer");
            }

            var result = await _assetService.GetAssetByIdAsync(assetId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Gets all assets for the current user
        /// </summary>
        /// <param name="userId">The ID of the user whose assets to retrieve</param>
        /// <returns>List of assets owned by the user</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUserAssets([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = await _assetService.GetAllAssetsByUserIdAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }/// <summary>
        /// Opens an asset to auction by changing its status from Draft to Open
        /// </summary>
        /// <param name="assetId">The ID of the asset to open for auction</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("{assetId:int}/open-to-auction")]
        public async Task<IActionResult> OpenToAuction(int assetId)
        {
            if (assetId <= 0)
            {
                return BadRequest("Asset ID must be a positive integer");
            }

            var result = await _assetService.OpenToAuctionAsync(new OpenAssetToAuctionRequest { AssetId = assetId });

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Asset opened for auction successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Handles error results and converts them to appropriate HTTP responses
        /// </summary>
        /// <param name="error">The error object to handle</param>
        /// <returns>Appropriate HTTP action result based on error code</returns>
        private IActionResult HandleErrorResult(Error error)
        {
            return error.ErrorCode switch
            {
                400 => BadRequest(new { error = error.Message, validationErrors = error.ValidationResults }),
                404 => NotFound(new { error = error.Message }),
                422 => UnprocessableEntity(new { error = error.Message, validationErrors = error.ValidationResults }),
                500 => StatusCode(500, new { error = "Internal server error", message = error.Message }),
                _ => StatusCode(500, new { error = "Unknown error occurred" })
            };
        }
    }
}
