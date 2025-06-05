public class DashboardResponse
{
    public List<AuctionResponse> LiveAuctions { get; set; } = new List<AuctionResponse>();
    public List<BidHistoryResponse> UserBids { get; set; } = new List<BidHistoryResponse>();
    public List<AuctionResponse> UserHighestBids { get; set; } = new List<AuctionResponse>();
}
