using System.ComponentModel.DataAnnotations;

public class PlaceBidRequest
{
    [Required]
    public int AuctionId { get; set; }

    [Required]
    public int BidderId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
    public int BidAmount { get; set; }
}
