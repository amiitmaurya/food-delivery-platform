using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.User;
using MiniSwiggy.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniSwiggy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UserController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? role)
    {
        var users = await _userService.GetAllUsersAsync(search, role);
        return Ok(users);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _userService.GetUserStatsAsync();
        return Ok(stats);
    }

    [HttpGet("roles")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var (success, message, userId) = await _userService.CreateUserAsync(request);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { id = userId, message });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id && request.Id != 0)
            request.Id = id;

        var (success, message) = await _userService.UpdateUserAsync(id, request);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, message) = await _userService.DeleteUserAsync(id, currentUserId);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var currentUserId = GetCurrentUserId();
        var (success, message, newStatus) = await _userService.ToggleUserStatusAsync(id, currentUserId);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message, isActive = newStatus });
    }

    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] AdminResetPasswordRequest request)
    {
        var (success, message) = await _userService.AdminResetPasswordAsync(id, request);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var userId))
            return userId;
        return 0;
    }
}
