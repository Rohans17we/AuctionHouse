using Microsoft.AspNetCore.Mvc;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.WebAPI.Controllers
{
    /// <summary>
    /// Dashboard controller for retrieving dashboard and summary information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Gets dashboard data for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Dashboard data including user statistics</returns>
        [HttpGet("{userId:int}")]
        public IActionResult GetDashboardData(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = _dashboardService.GetDashboardDataAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets live auctions sorted by expiry date
        /// </summary>
        /// <returns>List of live auctions ordered by expiry</returns>
        [HttpGet("live-auctions")]
        public IActionResult GetLiveAuctionsSortedByExpiry()
        {
            var result = _dashboardService.GetLiveAuctionsSortedByExpiryAsync();

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets auctions where the user has the highest bid
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of auctions where user has highest bid</returns>
        [HttpGet("user-highest-bids/{userId:int}")]
        public IActionResult GetUserHighestBids(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = _dashboardService.GetUserHighestBidsAsync(userId);

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
