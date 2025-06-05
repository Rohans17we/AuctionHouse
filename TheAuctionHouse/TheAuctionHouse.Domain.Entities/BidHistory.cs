namespace TheAuctionHouse.Domain.Entities;

public class BidHistory
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public int BidderName { get; set; }
    public int BidAmount { get; set; }
    public DateTime BidDate { get; set; }
}