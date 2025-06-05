using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Data.EFCore.InMemory;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.WebAPI.Controllers
{    /// <summary>
    /// Administrator controller for handling admin-specific operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdministratorService _administratorService;
        private readonly IAuctionService _auctionService;
        private readonly IPortalUserService _portalUserService;
        private readonly SqliteDbContext _dbContext;

        public AdminController(
            IAdministratorService administratorService,
            IAuctionService auctionService,
            IPortalUserService portalUserService,
            SqliteDbContext dbContext)
        {
            _administratorService = administratorService;
            _auctionService = auctionService;
            _portalUserService = portalUserService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Regenerates a user's password
        /// </summary>
        /// <param name="request">Password reset request</param>        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> RegenerateUserPassword([FromBody] AdminPasswordResetRequest request)
        {
            if (request == null)
            {
                return BadRequest("Password reset request cannot be null");
            }

            var result = await _administratorService.RegenerateUserPasswordAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "User password reset successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets audit log for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>        /// <returns>User audit log information</returns>
        [HttpGet("audit-log/{userId:int}")]
        public async Task<IActionResult> GetUserAuditLog(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = await _administratorService.GetUserAuditLogAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Manages user access (block/allow)
        /// </summary>
        /// <param name="request">User management request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("manage-user-access")]
        public async Task<IActionResult> ManageUserAccess([FromBody] AdminUserManagementRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            var result = await _administratorService.ManageUserAccessAsync(request);
            if (result.IsSuccess)
            {
                return Ok(new { success = true, message = $"User access has been {request.Action.ToLower()}ed successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="userId">The ID of the user to delete</param>        /// <returns>Boolean result indicating success or failure</returns>
        [HttpDelete("user/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }            var result = await _administratorService.DeleteUserAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "User deleted successfully" });
            }

            // Pass through the specific error message
            return BadRequest(new { success = false, message = result.Error.Message });
        }

        /// <summary>
        /// Gets audit logs for all users
        /// </summary>        /// <returns>List of all user audit logs</returns>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAllUsersAuditLog()
        {
            var result = await _administratorService.GetAllUsersAuditLogAsync();

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _portalUserService.GetAllUsersAsync();
            if (result.IsSuccess)
            {
                return Ok(new { success = true, data = result.Value });
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Gets live auctions
        /// </summary>
        /// <returns>List of live auctions</returns>
        [HttpGet("live-auctions")]
        public async Task<IActionResult> GetLiveAuctions()
        {
            var result = await _auctionService.GetLiveAuctionsAsync();
            if (result.IsSuccess)
            {
                return Ok(new { success = true, data = result.Value });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Truncates the Assets table
        /// </summary>
        /// <returns>Result of the operation</returns>
        [HttpPost("truncate-assets")]
        public async Task<IActionResult> TruncateAssets()
        {
            try
            {
                // Delete all records from Assets table and reset the auto-increment counter
                await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Assets; DELETE FROM sqlite_sequence WHERE name='Assets';");
                return Ok(new { success = true, message = "Assets table truncated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to truncate Assets table", message = ex.Message });
            }
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
