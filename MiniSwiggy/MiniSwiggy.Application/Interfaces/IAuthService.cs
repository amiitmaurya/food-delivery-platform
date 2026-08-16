using Microsoft.AspNetCore.Http;
using MiniSwiggy.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<string> UploadProfileImageAsync(int userId, IFormFile file);

    Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);

    Task<UserProfileDto?> GetUserProfileAsync(int userId);
}
