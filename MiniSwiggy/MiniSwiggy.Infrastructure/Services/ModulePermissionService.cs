using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.DTOs.Permission;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using MiniSwiggy.Infrastructure.Seed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Services;

public class ModulePermissionService : IModulePermissionService
{
    private readonly ApplicationDbContext _context;

    public ModulePermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserModulePermissionDto>> GetUserPermissionsAsync(int userId)
    {
        var permissions = await _context.UserModulePermissions
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderBy(p => p.Id)
            .ToListAsync();

        if (!permissions.Any())
        {
            await EnsureDefaultPermissionsForUserAsync(userId);
            permissions = await _context.UserModulePermissions
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        return permissions.Select(p => new UserModulePermissionDto
        {
            Id = p.Id,
            UserId = p.UserId,
            ModuleKey = p.ModuleKey,
            ModuleName = p.ModuleName,
            ModuleCategory = p.ModuleCategory,
            RoutePath = p.RoutePath,
            IconClass = p.IconClass,
            IsAllowed = p.IsAllowed
        }).ToList();
    }

    public async Task<IEnumerable<UserModulePermissionDto>> GetMyPermissionsAsync(int currentUserId)
    {
        return await GetUserPermissionsAsync(currentUserId);
    }

    public async Task<bool> UpdateUserPermissionsAsync(UpdateUserPermissionsRequest request)
    {
        if (request.UserId <= 0 || request.Permissions == null)
            return false;

        var existingPermissions = await _context.UserModulePermissions
            .Where(p => p.UserId == request.UserId && !p.IsDeleted)
            .ToListAsync();

        if (!existingPermissions.Any())
        {
            await EnsureDefaultPermissionsForUserAsync(request.UserId);
            existingPermissions = await _context.UserModulePermissions
                .Where(p => p.UserId == request.UserId && !p.IsDeleted)
                .ToListAsync();
        }

        foreach (var toggle in request.Permissions)
        {
            var target = existingPermissions.FirstOrDefault(p => p.ModuleKey.Equals(toggle.ModuleKey, StringComparison.OrdinalIgnoreCase));
            if (target != null)
            {
                target.IsAllowed = toggle.IsAllowed;
                target.UpdatedOn = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetUserPermissionsAsync(int userId)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        var roleName = user.Role?.Name?.ToLower() ?? "customer";

        var existing = await _context.UserModulePermissions.Where(p => p.UserId == userId).ToListAsync();
        _context.UserModulePermissions.RemoveRange(existing);
        await _context.SaveChangesAsync();

        await EnsureDefaultPermissionsForUserAsync(userId);
        return true;
    }

    private async Task EnsureDefaultPermissionsForUserAsync(int userId)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return;

        var roleName = user.Role?.Name?.ToLower() ?? "customer";

        foreach (var mod in ModulePermissionTableSeeder.SystemModules)
        {
            bool isAllowed = false;

            if (roleName.Contains("superadmin"))
            {
                isAllowed = true;
            }
            else if (roleName.Contains("admin"))
            {
                isAllowed = mod.Category == "Admin" || mod.Category == "SuperAdmin" || mod.Category == "Customer";
            }
            else if (roleName.Contains("delivery"))
            {
                isAllowed = mod.Key == "delivery_console" || mod.Key == "addresses";
            }
            else
            {
                isAllowed = mod.Category == "Customer";
            }

            _context.UserModulePermissions.Add(new UserModulePermission
            {
                UserId = userId,
                ModuleKey = mod.Key,
                ModuleName = mod.Name,
                ModuleCategory = mod.Category,
                RoutePath = mod.Route,
                IconClass = mod.Icon,
                IsAllowed = isAllowed,
                CreatedOn = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
