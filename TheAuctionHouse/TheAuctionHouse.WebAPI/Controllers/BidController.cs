using Microsoft.AspNetCore.Mvc;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.WebAPI.Controllers
{
    /// <summary>
    /// Bid management controller for handling bid-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;

        public BidController(IBidService bidService)
        {
            _bidService = bidService;
        }

        /// <summary>
        /// Places a new bid on an auction
        /// </summary>
        /// <param name="request">Bid placement request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest request)
        {
            if (request == null)
            {
                return BadRequest("Bid placement request cannot be null");
            }

            var result = await _bidService.PlaceBidAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Bid placed successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets bid history for a specific auction
        /// </summary>
        /// <param name="auctionId">The ID of the auction</param>
        /// <returns>List of bid history entries</returns>
        [HttpGet("auction/{auctionId:int}")]
        public async Task<IActionResult> GetBidHistoryByAuctionId(int auctionId)
        {
            if (auctionId <= 0)
            {
                return BadRequest("Auction ID must be a positive integer");
            }

            var result = await _bidService.GetBidHistoryByAuctionIdAsync(auctionId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets bid history for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of bid history entries</returns>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetBidHistoryByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = await _bidService.GetBidHistoryByUserIdAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Checks and unblocks previous bids for an auction
        /// </summary>
        /// <param name="auctionId">The ID of the auction</param>
        /// <param name="newHighestBidderId">The ID of the new highest bidder</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("unblock-previous/{auctionId:int}/{newHighestBidderId:int}")]
        public async Task<IActionResult> CheckAndUnblockPreviousBids(int auctionId, int newHighestBidderId)
        {
            if (auctionId <= 0)
            {
                return BadRequest("Auction ID must be a positive integer");
            }

            if (newHighestBidderId <= 0)
            {
                return BadRequest("New highest bidder ID must be a positive integer");
            }

            var result = await _bidService.CheckAndUnblockPreviousBidsAsync(auctionId, newHighestBidderId);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Previous bids unblocked successfully" });
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
