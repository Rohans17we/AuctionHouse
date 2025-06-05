using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Common.Validation;
using TheAuctionHouse.Common;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Domain.Entities;
using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

namespace TheAuctionHouse.Domain.Services;

public class AdministratorService : IAdministratorService
{
    private readonly IAppUnitOfWork _appUnitOfWork;

    public AdministratorService(IAppUnitOfWork appUnitOfWork)
    {
        _appUnitOfWork = appUnitOfWork;
    }    public async Task<Result<bool>> RegenerateUserPasswordAsync(AdminPasswordResetRequest request)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(request, validationError))
            {
                return Result<bool>.Failure(validationError);
            }

            // Check if user exists
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(request.UserId);
            if (user == null)
                return Result<bool>.Failure(Error.NotFound("User not found"));

            // Validate new password
            if (!PasswordHelper.IsValidPassword(request.NewPassword))
                return Result<bool>.Failure(Error.BadRequest("Password must be at least 6 characters long"));

            // Hash and update user password
            user.HashedPassword = PasswordHelper.HashPassword(request.NewPassword);
            await _appUnitOfWork.PortalUserRepository.UpdateAsync(user);
            await _appUnitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.InternalServerError(ex.Message));
        }
    }public async Task<Result<UserAuditLogResponse>> GetUserAuditLogAsync(int userId)
    {
        try
        {
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(userId);
            if (user == null)
                return Result<UserAuditLogResponse>.Failure(Error.NotFound("User not found"));

            // Create audit log response (in real implementation, audit logs would be tracked in database)
            var response = new UserAuditLogResponse
            {
                UserId = user.Id,
                UserName = user.Name,
                Email = user.EmailId,
                LastLoginDate = DateTime.UtcNow.AddDays(-1), // Placeholder
                ActivityLog = new List<string>
                {
                    "User registered",
                    "User logged in",
                    "User updated profile",
                    "User placed bid on auction"
                }, // Placeholder activities
                IsBlocked = user.IsBlocked
            };

            return Result<UserAuditLogResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UserAuditLogResponse>.Failure(Error.InternalServerError(ex.Message));
        }
    }    public async Task<Result<bool>> ManageUserAccessAsync(AdminUserManagementRequest request)
    {
        try
        {
            // Validate the request
            Error validationError = Error.ValidationFailures();
            if (!ValidationHelper.Validate(request, validationError))
            {
                return Result<bool>.Failure(validationError);
            }

            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(request.UserId);
            if (user == null)
                return Result<bool>.Failure(Error.NotFound("User not found"));

            // Validate action
            if (request.Action != "Block" && request.Action != "Allow")
                return Result<bool>.Failure(Error.BadRequest("Action must be either 'Block' or 'Allow'"));

            // Update user blocked status
            user.IsBlocked = request.Action == "Block";

            // Update user in database
            await _appUnitOfWork.PortalUserRepository.UpdateAsync(user);
            await _appUnitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(Error.InternalServerError(ex.Message));
        }
    }    public async Task<Result<bool>> DeleteUserAsync(int userId)
    {
        try
        {
            Console.WriteLine($"[DeleteUserAsync] Starting deletion for user ID: {userId}");
            var user = await _appUnitOfWork.PortalUserRepository.GetUserByUserIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine("[DeleteUserAsync] User not found");
                return Result<bool>.Failure(Error.NotFound("User not found"));
            }

            Console.WriteLine($"[DeleteUserAsync] Found user: {user.Name} (ID: {user.Id})");

            // Check if user has any blocked funds
            if (user.WalletBalanceBlocked > 0)
            {
                Console.WriteLine($"[DeleteUserAsync] User has blocked funds: {user.WalletBalanceBlocked}");
                return Result<bool>.Failure(Error.BadRequest("Cannot delete user with blocked funds"));
            }

            // Get open auctions where this user is the highest bidder
            var openAuctions = await _appUnitOfWork.AuctionRepository.GetAllAsync();
            var userActiveAuctions = openAuctions.Where(a => a.Status == AuctionStatus.Live && !a.IsExpired() && a.CurrentHighestBidderId == userId).ToList();
            if (userActiveAuctions.Any())
            {
                Console.WriteLine($"[DeleteUserAsync] User has active bids in {userActiveAuctions.Count} auctions");
                return Result<bool>.Failure(Error.BadRequest("Cannot delete user with active bids"));
            }

            // Get user's total funds
            var totalFunds = new BalanceInfo
            {
                TotalBalance = user.WalletBalance,
                BlockedAmount = user.WalletBalanceBlocked,
                AvailableBalance = user.WalletBalance - user.WalletBalanceBlocked
            };

            // If user has funds, don't allow deletion
            if (totalFunds.TotalBalance > 0)
            {
                Console.WriteLine($"[DeleteUserAsync] User has funds in wallet: {totalFunds.TotalBalance}");
                return Result<bool>.Failure(Error.BadRequest("Cannot delete user with funds in wallet. Please withdraw all funds first."));
            }

            Console.WriteLine("[DeleteUserAsync] All checks passed, proceeding with deletion");

            // Delete user
            await _appUnitOfWork.PortalUserRepository.DeleteAsync(user);
            await _appUnitOfWork.SaveChangesAsync();

            Console.WriteLine("[DeleteUserAsync] User deleted successfully");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteUserAsync] Error deleting user: {ex.Message}");
            Console.WriteLine($"[DeleteUserAsync] Stack trace: {ex.StackTrace}");
            return Result<bool>.Failure(Error.InternalServerError($"Failed to delete user: {ex.Message}"));
        }
    }    public async Task<Result<List<UserAuditLogResponse>>> GetAllUsersAuditLogAsync()
    {
        try
        {
            var allUsers = await _appUnitOfWork.PortalUserRepository.GetAllAsync();
            
            // Filter out admin users - only return regular platform users
            var regularUsers = allUsers.Where(user => !user.IsAdmin);

            var response = regularUsers.Select(user => new UserAuditLogResponse
            {
                UserId = user.Id,
                UserName = user.Name,
                Email = user.EmailId,
                LastLoginDate = DateTime.UtcNow.AddDays(-1), // Placeholder
                ActivityLog = new List<string>
                {
                    "User registered",
                    "User logged in"
                }, // Placeholder activities
                IsBlocked = user.IsBlocked,
                WalletBalance = user.WalletBalance,
                WalletBalanceBlocked = user.WalletBalanceBlocked
            }).ToList();

            return Result<List<UserAuditLogResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<List<UserAuditLogResponse>>.Failure(Error.InternalServerError(ex.Message));
        }
    }
}
