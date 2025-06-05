using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.WebAPI.Controllers
{    /// <summary>
    /// Wallet management controller for handling wallet-related operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Deposits funds into a wallet
        /// </summary>
        /// <param name="request">Wallet transaction request for deposit</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] WalletTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Wallet transaction request cannot be null");
            }

            var result = await _walletService.DepositAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Funds deposited successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Withdraws funds from a wallet
        /// </summary>
        /// <param name="request">Wallet transaction request for withdrawal</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WalletTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Wallet transaction request cannot be null");
            }

            var result = await _walletService.WithDrawalAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Funds withdrawn successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Gets wallet balance for a user
        /// </summary>
        /// <param name="userId">The ID of the user whose wallet balance to retrieve</param>
        /// <returns>Wallet balance information</returns>
        [HttpGet("balance/{userId:int}")]
        public async Task<IActionResult> GetWalletBalance(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }            var result = await _walletService.GetWalletBalanceAsync(userId);

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
        {            return error.ErrorCode switch
            {
                404 => NotFound(error.Message),
                400 => BadRequest(error.Message),
                422 => UnprocessableEntity(error.Message),
                _ => StatusCode(500, error.Message)
            };
        }
    }
}
