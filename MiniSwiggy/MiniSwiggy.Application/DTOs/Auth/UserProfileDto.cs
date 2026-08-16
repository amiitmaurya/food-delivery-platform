namespace MiniSwiggy.Application.DTOs.Auth;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? ImageUrl { get; set; }
    public string? CurrentPasswordHint { get; set; }
}
