using TheAuctionHouse.Common.ErrorHandling;

public interface IAdministratorService
{
    Task<Result<bool>> RegenerateUserPasswordAsync(AdminPasswordResetRequest request);
    Task<Result<UserAuditLogResponse>> GetUserAuditLogAsync(int userId);
    Task<Result<bool>> ManageUserAccessAsync(AdminUserManagementRequest request);
    Task<Result<bool>> DeleteUserAsync(int userId);
    Task<Result<List<UserAuditLogResponse>>> GetAllUsersAuditLogAsync();
}
