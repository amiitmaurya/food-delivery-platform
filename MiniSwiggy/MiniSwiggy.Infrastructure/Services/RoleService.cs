using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.DTOs.Role;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.Users)
            .AsNoTracking()
            .ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            UsersCount = r.Users?.Count(u => !u.IsDeleted) ?? 0
        });
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var r = await _context.Roles
            .Include(x => x.Users)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return null;

        return new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            UsersCount = r.Users?.Count(u => !u.IsDeleted) ?? 0
        };
    }

    public async Task<(bool Success, string Message, int? RoleId)> CreateRoleAsync(CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (false, "Role name is required.", null);

        var trimmedName = request.Name.Trim();
        if (await _context.Roles.AnyAsync(r => r.Name.ToLower() == trimmedName.ToLower()))
            return (false, "A role with this name already exists.", null);

        var role = new Role
        {
            Name = trimmedName,
            Description = request.Description?.Trim()
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return (true, "Role created successfully.", role.Id);
    }

    public async Task<(bool Success, string Message)> UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return (false, "Role not found.");

        var trimmedName = request.Name.Trim();
        if (await _context.Roles.AnyAsync(r => r.Name.ToLower() == trimmedName.ToLower() && r.Id != id))
            return (false, "Another role with this name already exists.");

        role.Name = trimmedName;
        role.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return (true, "Role updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return (false, "Role not found.");

        var builtInRoles = new[] { "SuperAdmin", "Admin", "Customer", "DeliveryPartner", "DeliveryBoy", "RestaurantOwner" };
        if (builtInRoles.Contains(role.Name, System.StringComparer.OrdinalIgnoreCase))
            return (false, $"System role '{role.Name}' is a core system role and cannot be deleted.");

        var activeUsers = role.Users?.Count(u => !u.IsDeleted) ?? 0;
        if (activeUsers > 0)
            return (false, $"Cannot delete role '{role.Name}' because {activeUsers} user(s) are currently assigned to it.");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return (true, $"Role '{role.Name}' deleted successfully.");
    }
}
