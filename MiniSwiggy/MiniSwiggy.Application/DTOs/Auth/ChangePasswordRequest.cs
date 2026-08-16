namespace MiniSwiggy.Application.DTOs.Auth;

public class ChangePasswordRequest
{
    public string? OldPassword { get; set; }
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
