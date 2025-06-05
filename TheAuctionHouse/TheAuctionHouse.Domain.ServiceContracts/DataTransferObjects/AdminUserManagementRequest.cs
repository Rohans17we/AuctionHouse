public class AdminUserManagementRequest
{
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty; // "Block", "Allow", "Delete"
}
