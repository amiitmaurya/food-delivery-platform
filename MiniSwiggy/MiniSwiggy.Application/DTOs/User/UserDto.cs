using System;

namespace MiniSwiggy.Application.DTOs.User;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public DateTime? LastLogin { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedOn { get; set; }
    public int OrdersCount { get; set; }
    public int AddressesCount { get; set; }
    public int ReviewsCount { get; set; }
}
