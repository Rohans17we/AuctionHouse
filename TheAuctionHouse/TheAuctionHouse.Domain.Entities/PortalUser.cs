namespace TheAuctionHouse.Domain.Entities;

public class PortalUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmailId { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public int WalletBalance { get; set; }
    public int WalletBalanceBlocked { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsBlocked { get; set; }
}
