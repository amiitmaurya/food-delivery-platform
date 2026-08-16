using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.DTOs.User;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? searchQuery = null, string? roleFilter = null)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Orders)
            .Include(u => u.Addresses)
            .Include(u => u.Reviews)
            .Where(u => !u.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var q = searchQuery.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(q) ||
                u.Email.ToLower().Contains(q) ||
                u.PhoneNumber.ToLower().Contains(q) ||
                (u.Role != null && u.Role.Name.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All")
        {
            var rf = roleFilter.Trim().ToLower();
            query = query.Where(u => u.Role != null && u.Role.Name.ToLower() == rf);
        }

        var users = await query.OrderByDescending(u => u.Id).ToListAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            RoleName = u.Role?.Name ?? "Customer",
            IsActive = u.IsActive,
            EmailVerified = u.EmailVerified,
            LastLogin = u.LastLogin,
            ImageUrl = u.ImageUrl,
            CreatedOn = u.CreatedOn,
            OrdersCount = u.Orders?.Count ?? 0,
            AddressesCount = u.Addresses?.Count ?? 0,
            ReviewsCount = u.Reviews?.Count ?? 0
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var u = await _context.Users
            .Include(x => x.Role)
            .Include(x => x.Orders)
            .Include(x => x.Addresses)
            .Include(x => x.Reviews)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (u == null) return null;

        return new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            RoleName = u.Role?.Name ?? "Customer",
            IsActive = u.IsActive,
            EmailVerified = u.EmailVerified,
            LastLogin = u.LastLogin,
            ImageUrl = u.ImageUrl,
            CreatedOn = u.CreatedOn,
            OrdersCount = u.Orders?.Count ?? 0,
            AddressesCount = u.Addresses?.Count ?? 0,
            ReviewsCount = u.Reviews?.Count ?? 0
        };
    }

    public async Task<(bool Success, string Message, int? UserId)> CreateUserAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return (false, "Email address is required.", null);

        if (string.IsNullOrWhiteSpace(request.FullName))
            return (false, "Full name is required.", null);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email && !u.IsDeleted))
            return (false, "A user with this email address already exists.", null);

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && !u.IsDeleted))
            return (false, "A user with this phone number already exists.", null);

        var role = await _context.Roles.FindAsync(request.RoleId);
        if (role == null)
            return (false, "Specified role was not found.", null);

        var password = string.IsNullOrWhiteSpace(request.Password) ? "Password123!" : request.Password;

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
            PasswordHash = _passwordHasher.HashPassword(password),
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            EmailVerified = true,
            ImageUrl = request.ImageUrl,
            CreatedOn = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "User created successfully.", user.Id);
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            return (false, "User not found.");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id && !u.IsDeleted))
            return (false, "Another user with this email already exists.");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            await _context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != id && !u.IsDeleted))
            return (false, "Another user with this phone number already exists.");

        var role = await _context.Roles.FindAsync(request.RoleId);
        if (role == null)
            return (false, "Specified role was not found.");

        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim().ToLower();
        user.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;
        user.EmailVerified = request.EmailVerified;
        user.UpdatedOn = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
            user.ImageUrl = request.ImageUrl;

        await _context.SaveChangesAsync();

        return (true, "User updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(int id, int currentUserId)
    {
        if (id == currentUserId)
            return (false, "You cannot delete your own logged-in account.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
            return (false, "User not found.");

        // Check if deleting last super admin
        if (user.Role?.Name == "SuperAdmin")
        {
            var superAdminCount = await _context.Users
                .Include(u => u.Role)
                .CountAsync(u => u.Role != null && u.Role.Name == "SuperAdmin" && !u.IsDeleted);

            if (superAdminCount <= 1)
                return (false, "Cannot delete the last remaining Super Administrator.");
        }

        // Soft delete user to maintain order history integrity
        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, $"User '{user.FullName}' was deleted successfully.");
    }

    public async Task<(bool Success, string Message, bool NewStatus)> ToggleUserStatusAsync(int id, int currentUserId)
    {
        if (id == currentUserId)
            return (false, "You cannot change the active status of your own account.", true);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            return (false, "User not found.", false);

        user.IsActive = !user.IsActive;
        user.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, $"User status changed to {(user.IsActive ? "Active" : "Inactive")}.", user.IsActive);
    }

    public async Task<(bool Success, string Message)> AdminResetPasswordAsync(int id, AdminResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return (false, "Password must be at least 6 characters long.");

        if (request.NewPassword != request.ConfirmPassword)
            return (false, "New password and confirmation password do not match.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null)
            return (false, "User not found.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "Password has been reset successfully.");
    }

    public async Task<UserStatsDto> GetUserStatsAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        return new UserStatsDto
        {
            TotalUsers = users.Count,
            TotalSuperAdmins = users.Count(u => u.Role?.Name == "SuperAdmin"),
            TotalAdmins = users.Count(u => u.Role?.Name == "Admin"),
            TotalCustomers = users.Count(u => u.Role?.Name == "Customer"),
            TotalDeliveryPartners = users.Count(u => u.Role?.Name == "DeliveryPartner" || u.Role?.Name == "DeliveryBoy"),
            TotalRestaurantOwners = users.Count(u => u.Role?.Name == "RestaurantOwner"),
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive)
        };
    }
}
