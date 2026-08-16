using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Auth;

public class AuthResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Token { get; set; }
    public string? FullName { get; set; }

    public string? ImageUrl { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Role { get; set; }
} 