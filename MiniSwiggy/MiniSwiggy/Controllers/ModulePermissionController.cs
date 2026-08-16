using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Permission;
using MiniSwiggy.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniSwiggy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ModulePermissionController : ControllerBase
{
    private readonly IModulePermissionService _permissionService;

    public ModulePermissionController(IModulePermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    private int GetCurrentUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }

    // Get current logged-in user permissions (Used for dynamic Sidebar & Route Guards)
    [HttpGet("my-permissions")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = GetCurrentUserId();
        var result = await _permissionService.GetMyPermissionsAsync(userId);
        return Ok(result);
    }

    // Get permissions for a specific user (SuperAdmin & Admin access)
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var result = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(result);
    }

    // Update permissions for a user (SuperAdmin)
    [HttpPost("update")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateUserPermissions([FromBody] UpdateUserPermissionsRequest request)
    {
        if (request == null || request.UserId <= 0)
            return BadRequest(new { message = "Invalid user permission payload." });

        var success = await _permissionService.UpdateUserPermissionsAsync(request);
        if (!success)
            return BadRequest(new { message = "Failed to update permissions in database." });

        return Ok(new { message = "User module permissions updated successfully in database!" });
    }

    // Reset user permissions to role defaults
    [HttpPost("reset/{userId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ResetUserPermissions(int userId)
    {
        var success = await _permissionService.ResetUserPermissionsAsync(userId);
        if (!success)
            return BadRequest(new { message = "Failed to reset permissions in database." });

        return Ok(new { message = "User permissions reset to role defaults in database." });
    }
}
