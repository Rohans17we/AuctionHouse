using System.Threading.Tasks;
using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Common;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;

namespace TheAuctionHouse.Domain.Services;

public class PortalUserService : IPortalUserService
{
    private IAppUnitOfWork _appUnitOfWork;
    private IEmailService _emailService;
    public PortalUserService(IAppUnitOfWork appUnitOfWork, IEmailService emailService)
    {
        _appUnitOfWork = appUnitOfWork;
        _emailService = emailService;
    }    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
    {
        try
        {
            //Check if the email id is not null or empty.
            //validate if the email address is in the expected format.
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(forgotPasswordRequest, validationError))
            {
                return Result<bool>.Failure(validationError);
            }
            //Check if the email id exists.
            PortalUser? portalUser = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(forgotPasswordRequest.EmailId);

            if (portalUser == null)
                return Result<bool>.Failure(Error.NotFound("User Not Found."));

            //Send a reset password link to the email id.
            //await _emailService.SendEmailAsync(portalUser.EmailId, "Password Reset | The Auction House", $"Dear {portalUser.Name}, \n <a href=\"https://TheAuctionHouse/ResetPassword\" click here to reset the password. \n Regards Admin Team", true);
            await _emailService.SendEmailAsync(portalUser.EmailId, "Password Reset | The Auction House", $"", true);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.InternalServerError(ex.Message));
        }
    }    public async Task<Result<bool>> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            // Log login attempt details
            Console.WriteLine($"[LoginAsync] Starting login attempt for email: {loginRequest?.EmailId ?? "null"}");

            // Validate the request
            if (loginRequest == null)
            {
                Console.WriteLine("[LoginAsync] Login request is null");
                return Result<bool>.Failure(Error.BadRequest("Login request cannot be null"));
            }

            if (string.IsNullOrWhiteSpace(loginRequest.EmailId) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                Console.WriteLine("[LoginAsync] Email or password is empty");
                return Result<bool>.Failure(Error.BadRequest("Email and password are required"));
            }

            // Additional email format validation
            if (!loginRequest.EmailId.Contains("@") || loginRequest.EmailId.Length < 5)
            {
                Console.WriteLine("[LoginAsync] Invalid email format");
                return Result<bool>.Failure(Error.BadRequest("Invalid email format"));
            }

            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(loginRequest, validationError))
            {
                Console.WriteLine("[LoginAsync] Validation failed");
                return Result<bool>.Failure(validationError);
            }

            try
            {
                // Get user by email
                Console.WriteLine($"[LoginAsync] Fetching user from database for email: {loginRequest.EmailId.Trim().ToLower()}");
                var user = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(loginRequest.EmailId.Trim().ToLower());
                
                if (user == null)
                {
                    Console.WriteLine("[LoginAsync] User not found in database");
                    return Result<bool>.Failure(Error.BadRequest("Invalid email or password"));
                }
                
                Console.WriteLine($"[LoginAsync] User found in database, ID: {user.Id}");
                
                // Check if user is blocked
                if (user.IsBlocked)
                {
                    Console.WriteLine("[LoginAsync] User is blocked");
                    return Result<bool>.Failure(Error.BadRequest("Your account has been blocked. Please contact support."));
                }

                // Verify password
                try
                {
                    // Log password details (first few chars of hash only)
                    Console.WriteLine($"[LoginAsync] Stored hash prefix: {user.HashedPassword?.Substring(0, Math.Min(10, user.HashedPassword?.Length ?? 0))}...");
                    
                    bool isPasswordValid = PasswordHelper.VerifyPassword(loginRequest.Password, user.HashedPassword);
                    Console.WriteLine($"[LoginAsync] Password verification result: {isPasswordValid}");
                    
                    if (!isPasswordValid)
                    {
                        Console.WriteLine("[LoginAsync] Password verification failed");
                        return Result<bool>.Failure(Error.BadRequest("Invalid email or password"));
                    }
                    Console.WriteLine("[LoginAsync] Password verified successfully");
                }
                catch (Exception pwEx)
                {
                    Console.WriteLine($"[LoginAsync] Password verification error: {pwEx.Message}");
                    Console.WriteLine($"[LoginAsync] Stack trace: {pwEx.StackTrace}");
                    return Result<bool>.Failure(Error.InternalServerError("An error occurred during authentication"));
                }

                return Result<bool>.Success(true);
            }
            catch (Exception dbEx)
            {
                Console.WriteLine($"[LoginAsync] Database error: {dbEx.Message}");
                Console.WriteLine($"[LoginAsync] Stack trace: {dbEx.StackTrace}");
                return Result<bool>.Failure(Error.InternalServerError("An error occurred while accessing user data"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoginAsync] Unexpected error: {ex.Message}");
            Console.WriteLine($"[LoginAsync] Stack trace: {ex.StackTrace}");
            return Result<bool>.Failure(Error.InternalServerError("An unexpected error occurred. Please try again later."));
        }
    }public async Task<Result<bool>> LogoutAsync(int UserId)
    {
        try
        {
            // Validate user exists
            var user = await _appUnitOfWork.PortalUserRepository.GetByIdAsync(UserId);
            if (user == null)
            {
                return Result<bool>.Failure(Error.NotFound("User not found"));
            }

            // In a more complex system, you might want to:
            // 1. Invalidate any active sessions
            // 2. Clear any refresh tokens
            // 3. Log the logout event
            // For now, we'll just return success as the frontend handles the cleanup
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.InternalServerError(ex.Message));
        }
    }public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();            if (!ValidationHelper.Validate(resetPasswordRequest, validationError))
            {
                return Result<bool>.Failure(validationError);
            }

            // Validate password confirmation
            if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmPassword)
            {
                return Result<bool>.Failure(Error.BadRequest("New password and confirm password do not match"));
            }

            // Validate new password strength
            if (!PasswordHelper.IsValidPassword(resetPasswordRequest.NewPassword))
            {
                return Result<bool>.Failure(Error.BadRequest("New password must be at least 6 characters long"));
            }

            // Get user by ID
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(resetPasswordRequest.UserId);
            if (user == null)
            {
                return Result<bool>.Failure(Error.NotFound("User not found"));
            }

            // Verify old password
            if (!PasswordHelper.VerifyPassword(resetPasswordRequest.OldPassword, user.HashedPassword))
            {
                return Result<bool>.Failure(Error.BadRequest("Current password is incorrect"));
            }

            // Hash and update new password
            user.HashedPassword = PasswordHelper.HashPassword(resetPasswordRequest.NewPassword);
            await _appUnitOfWork.PortalUserRepository.UpdateAsync(user);
            await _appUnitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.InternalServerError(ex.Message));
        }    }

    public async Task<Result<bool>> SignUpAsync(SignUpRequest signUpRequest)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(signUpRequest, validationError))
            {
                return Result<bool>.Failure(validationError);
            }

            // Validate password strength
            if (!PasswordHelper.IsValidPassword(signUpRequest.Password))
            {
                return Result<bool>.Failure(Error.BadRequest("Password must be at least 6 characters long"));
            }            // Check if email already exists
            var existingUser = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(signUpRequest.EmailId);
            if (existingUser != null)
            {
                return Result<bool>.Failure(Error.BadRequest("Email address is already registered"));
            }

            // Hash the password before storing
            string hashedPassword = PasswordHelper.HashPassword(signUpRequest.Password);

            // Create new user
            var user = new PortalUser
            {
                Name = signUpRequest.Name,
                EmailId = signUpRequest.EmailId,
                HashedPassword = hashedPassword,
                IsAdmin = false,
                WalletBalance = 0,
                WalletBalanceBlocked = 0,
                IsBlocked = false
            };
            await _appUnitOfWork.PortalUserRepository.AddAsync(user);
            await _appUnitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.InternalServerError(ex.Message));
        }
    }    public async Task<Result<PortalUser>> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByEmailAsync(email);
            if (user == null)
                return Result<PortalUser>.Failure(Error.NotFound("User not found"));

            return Result<PortalUser>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<PortalUser>.Failure(Error.InternalServerError(ex.Message));
        }
    }    public async Task<Result<IEnumerable<PortalUser>>> GetAllUsersAsync()
    {
        try
        {
            var allUsers = await _appUnitOfWork.PortalUserRepository.GetAllAsync();
            // Filter out admin users - only return regular platform users
            var regularUsers = allUsers.Where(user => !user.IsAdmin);
            return Result<IEnumerable<PortalUser>>.Success(regularUsers);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PortalUser>>.Failure(Error.InternalServerError(ex.Message));
        }
    }
}
