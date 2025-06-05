using System.ComponentModel.DataAnnotations;

public class PostAuctionRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Asset ID must be a positive integer")]
    public int AssetId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Owner ID must be a positive integer")]  
    public int OwnerId { get; set; }

    [Required]
    [Range(1, 9999, ErrorMessage = "Reserve price must be between $1 and $9,999")]
    public int ReservedPrice { get; set; }

    [Required]
    [Range(0, 9999, ErrorMessage = "Current highest bid must be between $0 and $9,999")]
    public int CurrentHighestBid { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Current highest bidder ID must be 0 or a positive integer")]
    public int CurrentHighestBidderId { get; set; }

    [Required]
    [Range(1, 999, ErrorMessage = "Minimum bid increment must be between $1 and $999")]
    public int MinimumBidIncrement { get; set; }

    [Required]
    [Range(1, 10080, ErrorMessage = "Duration must be between 1 minute and 10080 minutes (7 days)")]
    public int TotalMinutesToExpiry { get; set; }
}