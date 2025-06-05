using TheAuctionHouse.Common.ErrorHandling;

public interface IBidService
{
    Task<Result<bool>> PlaceBidAsync(PlaceBidRequest placeBidRequest);
    Task<Result<List<BidHistoryResponse>>> GetBidHistoryByAuctionIdAsync(int auctionId);
    Task<Result<List<BidHistoryResponse>>> GetBidHistoryByUserIdAsync(int userId);
    Task<Result<bool>> CheckAndUnblockPreviousBidsAsync(int auctionId, int newHighestBidderId);
}
