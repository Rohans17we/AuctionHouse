public class UserAuditLogResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LastLoginDate { get; set; }
    public List<string> ActivityLog { get; set; } = new List<string>();
    public bool IsBlocked { get; set; }
    public int WalletBalance { get; set; }
    public int WalletBalanceBlocked { get; set; }
}
