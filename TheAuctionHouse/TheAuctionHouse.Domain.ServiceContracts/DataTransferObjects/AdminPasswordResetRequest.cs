public class AdminPasswordResetRequest
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
