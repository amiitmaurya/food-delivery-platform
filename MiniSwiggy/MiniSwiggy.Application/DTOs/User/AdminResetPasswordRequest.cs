namespace MiniSwiggy.Application.DTOs.User;

public class AdminResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
