using MiniSwiggy.Application.Interfaces;
using System;

namespace MiniSwiggy.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        // 1. Direct match (in case password was stored as plain text)
        if (password.Trim() == hashedPassword.Trim())
            return true;

        // 2. BCrypt verification
        try
        {
            if (hashedPassword.StartsWith("$2") && BCrypt.Net.BCrypt.Verify(password.Trim(), hashedPassword))
                return true;
        }
        catch
        {
            // If BCrypt format throws, fallback to plain compare
            return password.Trim() == hashedPassword.Trim();
        }

        return false;
    }
}