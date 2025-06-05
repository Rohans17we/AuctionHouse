using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.WebAPI.Services;

namespace TheAuctionHouse.WebAPI.Controllers
{    /// <summary>
    /// User controller for handling portal user operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly IPortalUserService _portalUserService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserController> _logger;

        public UserController(IPortalUserService portalUserService, ITokenService tokenService, ILogger<UserController> logger)
        {
            _portalUserService = portalUserService;
            _tokenService = tokenService;
            _logger = logger;
        }        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">User registration request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, error = "Sign up request cannot be null" });
            }

            var result = await _portalUserService.SignUpAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = true, message = "User registered successfully" });
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="request">User login request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login request received: {@LoginRequest}", new { 
                    EmailId = request?.EmailId,
                    PasswordProvided = !string.IsNullOrEmpty(request?.Password),
                    RequestNull = request == null,
                    EmailEmpty = string.IsNullOrWhiteSpace(request?.EmailId),
                    PasswordEmpty = string.IsNullOrWhiteSpace(request?.Password)
                });

                if (request == null)
                {
                    _logger.LogWarning("Login request was null");
                    return BadRequest(new { success = false, error = "Login request cannot be null" });
                }

                if (string.IsNullOrWhiteSpace(request.EmailId) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Login request missing required fields. EmailId: {EmailProvided}, Password: {PasswordProvided}", 
                        !string.IsNullOrWhiteSpace(request.EmailId),
                        !string.IsNullOrWhiteSpace(request.Password));
                    return BadRequest(new { success = false, error = "Email and password are required" });
                }

                var result = await _portalUserService.LoginAsync(request);                _logger.LogInformation("Login result: {@Result}", new { Success = result.IsSuccess, Error = result.Error?.Message });
                if (!result.IsSuccess)
                {
                    return HandleErrorResult(result.Error);
                }

                _logger.LogInformation("Getting user details for {Email}", request.EmailId);                var userResult = await _portalUserService.GetUserByEmailAsync(request.EmailId);
                if (!userResult.IsSuccess)
                {
                    return HandleErrorResult(userResult.Error ?? Error.InternalServerError("User not found after successful login"));
                }

                var user = userResult.Value;
                if (user == null)
                {
                    return HandleErrorResult(Error.InternalServerError("User not found after successful login"));
                }                string token;
                try
                {
                    token = _tokenService.GenerateToken(user);
                }                catch (Exception tokenEx)
                {
                    _logger.LogError(tokenEx, "Token generation failed: {ErrorMessage}", tokenEx.Message);
                    var innerMessage = tokenEx.InnerException?.Message ?? tokenEx.Message;
                    return StatusCode(500, new { success = false, error = "Failed to generate authentication token", message = innerMessage });
                }

                return Ok(new { 
                    success = true, 
                    message = "User logged in successfully",
                    token = token,
                    user = new {
                        id = user.Id,
                        name = user.Name,
                        email = user.EmailId,
                        isAdmin = user.IsAdmin
                    }
                });
            }            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during login: {ErrorMessage}", ex.Message);
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { success = false, error = "An unexpected error occurred during login", message = innerMessage });
            }
        }/// <summary>
        /// Logs out a user
        /// </summary>
        /// <param name="userId">The ID of the user to log out</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [Authorize]
        [HttpPost("logout/{userId:int}")]
        public async Task<IActionResult> Logout(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("User ID must be a positive integer");
            }

            var result = await _portalUserService.LogoutAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "User logged out successfully" });
            }

            return HandleErrorResult(result.Error);
        }

        /// <summary>
        /// Initiates password reset process
        /// </summary>
        /// <param name="request">Forgot password request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Forgot password request cannot be null");
            }

            var result = await _portalUserService.ForgotPasswordAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Password reset email sent successfully" });
            }

            return HandleErrorResult(result.Error);
        }        /// <summary>
        /// Resets user password
        /// </summary>
        /// <param name="request">Password reset request</param>
        /// <returns>Boolean result indicating success or failure</returns>
        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Reset password request cannot be null");
            }

            var result = await _portalUserService.ResetPasswordAsync(request);

            if (result.IsSuccess)
            {
                return Ok(new { success = result.Value, message = "Password reset successfully" });
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
            var response = new 
            { 
                success = false,
                error = error.Message,
                errorCode = error.ErrorCode
            };

            return error.ErrorCode switch
            {
                400 => BadRequest(response),
                401 => Unauthorized(response),
                403 => StatusCode(403, response), // Use explicit status code for consistency
                404 => NotFound(response),
                422 => UnprocessableEntity(response),
                _ => StatusCode(500, new { success = false, error = "Internal server error", message = error.Message })
            };
        }
    }
}
