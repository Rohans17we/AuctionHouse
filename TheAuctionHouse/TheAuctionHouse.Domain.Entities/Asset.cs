namespace TheAuctionHouse.Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RetailValue { get; set; }
    public AssetStatus Status { get; set; }
}

public enum AssetStatus
{
    Draft,
    Open,           // Asset is available for auction
    Active,         // Asset is in an active auction
    ClosedForAuction // Reserved for internal use during ownership transfer
}