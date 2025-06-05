namespace TheAuctionHouse.Domain.ServiceContracts.DataTransferObjects;

public class BalanceInfo
{
    public int TotalBalance { get; set; }
    public int BlockedAmount { get; set; }
    public int AvailableBalance { get; set; }
}
