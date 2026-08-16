using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Role;
using MiniSwiggy.Application.Interfaces;
using System.Threading.Tasks;

namespace MiniSwiggy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
            return NotFound(new { message = "Role not found." });

        return Ok(role);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var (success, message, roleId) = await _roleService.CreateRoleAsync(request);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { id = roleId, message });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request)
    {
        if (id != request.Id && request.Id != 0)
            request.Id = id;

        var (success, message) = await _roleService.UpdateRoleAsync(id, request);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _roleService.DeleteRoleAsync(id);
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }
}
