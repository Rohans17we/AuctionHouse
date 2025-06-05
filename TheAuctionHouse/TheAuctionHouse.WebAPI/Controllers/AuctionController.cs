using Microsoft.AspNetCore.Mvc;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.WebAPI.Controllers
{
    /// <summary>
    /// Auction management controller for handling auction-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;

        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }        /// <summary>
        /// Posts a new auction
        /// </summary>
        /// <param name="request">Auction posting request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost]        
        public IActionResult PostAuction([FromBody] PostAuctionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Auction posting request cannot be null");
            }

            var result = _auctionService.PostAuctionAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Auction posted successfully" });
            }

            return HandleErrorResult(result.Error);
        }/// <summary>
        /// Checks and processes auction expiries
        /// </summary>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("check-expiries")]
        public async Task<IActionResult> CheckAuctionExpiries()
        {
            var result = await _auctionService.CheckAuctionExpiriesAsync();

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Auction expiries checked successfully" });
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Gets an auction by ID
        /// </summary>
        /// <param name="auctionId">The ID of the auction to retrieve</param>
        /// <returns>Auction details</returns>
        [HttpGet("{auctionId:int}")]        
        public IActionResult GetAuctionById(int auctionId)
        {
            if (auctionId <= 0)
            {
                return BadRequest("Auction ID must be a positive integer");
            }

            var result = _auctionService.GetAuctionByIdAsync(auctionId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Gets all auctions for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user whose auctions to retrieve</param>
        /// <returns>List of auctions</returns>
        [HttpGet("user/{userId:int}")]
        public IActionResult GetAuctionsByUserId(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = _auctionService.GetAuctionsByUserIdAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Gets all open auctions for the current user
        /// </summary>
        /// <returns>List of open auctions</returns>
        [HttpGet("open")]
        public IActionResult GetAllOpenAuctions()
        {
            var result = _auctionService.GetAllOpenAuctionsByUserIdAsync();

            if (result.IsSuccess)
            {
                return Ok(result.Value);
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
