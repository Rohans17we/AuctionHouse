using TheAuctionHouse.Common.ErrorHandling;
using TheAuctionHouse.Domain.Entities;

namespace TheAuctionHouse.Domain.ServiceContracts
{
    public interface IAuctionService
    {
        Result<bool> PostAuctionAsync(PostAuctionRequest postAuctionRequest);
        Task<Result<bool>> CheckAuctionExpiriesAsync();
        Result<AuctionResponse> GetAuctionByIdAsync(int auctionId);
        Result<List<AuctionResponse>> GetAuctionsByUserIdAsync(int userId);
        Result<List<AuctionResponse>> GetAllOpenAuctionsByUserIdAsync();
        Task<Result<IEnumerable<Auction>>> GetLiveAuctionsAsync();
    }
}