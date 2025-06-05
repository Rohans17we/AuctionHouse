using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.WebAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(PortalUser user);
    }
}
