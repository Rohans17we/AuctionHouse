public class WalletBalanceResponse
{
    public int UserId { get; set; }
    public int Amount { get; set; }
    public int BlockedAmount { get; set; }
    public int AvailableAmount => Amount - BlockedAmount;
    public List<BidHistoryResponse> BidHistory { get; set; } = new List<BidHistoryResponse>();
}