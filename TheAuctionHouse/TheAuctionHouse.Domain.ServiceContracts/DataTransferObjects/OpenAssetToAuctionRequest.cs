using System.ComponentModel.DataAnnotations;

public class OpenAssetToAuctionRequest
{
    [Required]
    public int AssetId { get; set; }
}
