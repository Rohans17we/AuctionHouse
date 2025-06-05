using TheAuctionHouse.Common.ErrorHandling;

public interface IDashboardService
{
    Result<DashboardResponse> GetDashboardDataAsync(int userId);
    Result<List<AuctionResponse>> GetLiveAuctionsSortedByExpiryAsync();
    Result<List<AuctionResponse>> GetUserHighestBidsAsync(int userId);
}
