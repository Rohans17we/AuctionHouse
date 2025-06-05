using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Domain.ServiceContracts;

public interface IPortalUserService
{
    Task<Result<bool>> SignUpAsync(SignUpRequest signUpRequest);
    Task<Result<bool>> LoginAsync(LoginRequest loginRequest);
    Task<Result<bool>> LogoutAsync(int UserId);
    Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
    Task<Result<PortalUser>> GetUserByEmailAsync(string email);
    Task<Result<IEnumerable<PortalUser>>> GetAllUsersAsync();
}
