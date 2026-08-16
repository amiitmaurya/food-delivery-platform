using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MiniSwiggy.Application.DTOs.Auth;
using MiniSwiggy.Application.Interfaces;

namespace MiniSwiggy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("upload-profile-image")]
    [Authorize]
    public async Task<IActionResult> UploadProfileImage(
    [FromForm] UploadUserProfileImageRequest request)
    {
        var userId = int.Parse(
    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var imageUrl = await _authService.UploadProfileImageAsync(
            userId,
            request.File);

        return Ok(new
        {
            Message = "Profile image uploaded successfully.",
            ImageUrl = imageUrl
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _authService.UpdateProfileAsync(userId, request);

        if (!result)
            return NotFound(new { message = "User not found." });

        return Ok(new { message = "Profile updated successfully." });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var profile = await _authService.GetUserProfileAsync(userId);

        if (profile is null)
            return NotFound(new { message = "User not found." });

        return Ok(profile);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { message = "New password must be at least 6 characters long." });

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { message = "New password and confirm password do not match." });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _authService.ChangePasswordAsync(userId, request);

        if (!result)
            return BadRequest(new { message = "Current password entered is incorrect." });

        return Ok(new { message = "Password updated successfully in database." });
    }
} 