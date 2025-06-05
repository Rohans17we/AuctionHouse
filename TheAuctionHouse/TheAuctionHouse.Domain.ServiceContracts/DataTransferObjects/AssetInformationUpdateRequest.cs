using System.ComponentModel.DataAnnotations;

public class AssetInformationUpdateRequest
{
    public int AssetId { get; set; }
    
    public int OwnerId { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(150, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 150 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Title can only contain letters, numbers, and spaces")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Retail price is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Retail price must be a positive integer")]
    public int RetailPrice { get; set; }
}