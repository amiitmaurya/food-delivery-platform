using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Seed;

public static class ModulePermissionTableSeeder
{
    public static readonly List<(string Key, string Name, string Category, string Route, string Icon)> SystemModules = new()
    {
        // Customer Modules
        ("restaurants", "Explore Restaurants", "Customer", "/restaurant", "fa-solid fa-store"),
        ("cart", "Shopping Cart", "Customer", "/cart", "fa-solid fa-basket-shopping"),
        ("orders", "My Orders", "Customer", "/my-orders", "fa-solid fa-clock-rotate-left"),
        ("wishlist", "Wishlist & Favorites", "Customer", "/wishlist", "fa-solid fa-heart"),
        ("addresses", "Saved Addresses", "Customer", "/addresses", "fa-solid fa-location-dot"),

        // Admin Modules
        ("admin_dashboard", "Admin Dashboard", "Admin", "/dashboard", "fa-solid fa-chart-line"),
        ("restaurant_master", "Restaurant Master", "Admin", "/admin/restaurants", "fa-solid fa-store"),
        ("category_master", "Category Master", "Admin", "/categories", "fa-solid fa-layer-group"),
        ("food_master", "Food Item Master", "Admin", "/food-item", "fa-solid fa-burger"),
        ("order_master", "Order Master", "Admin", "/admin/orders", "fa-solid fa-boxes-packing"),
        ("coupon_master", "Coupon & Promo Master", "Admin", "/coupons", "fa-solid fa-ticket"),

        // Super Admin Modules
        ("superadmin_dashboard", "Master Command Center", "SuperAdmin", "/superadmin/dashboard", "fa-solid fa-gauge-high"),
        ("user_master", "User Master Directory", "SuperAdmin", "/superadmin/users", "fa-solid fa-users-gear"),
        ("role_master", "Role Master", "SuperAdmin", "/superadmin/roles", "fa-solid fa-shield-halved"),
        ("fleet_master", "Delivery Partner Fleet Master", "SuperAdmin", "/superadmin/delivery-partners", "fa-solid fa-person-biking"),
        ("review_master", "Review & Rating Master", "SuperAdmin", "/superadmin/reviews", "fa-solid fa-comments"),
        ("permission_master", "User Module Access Master", "SuperAdmin", "/superadmin/permissions", "fa-solid fa-key"),

        // Rider Modules
        ("delivery_console", "Rider Delivery Console", "Delivery", "/delivery-partner/dashboard", "fa-solid fa-motorcycle")
    };

    public static async Task EnsureTablesAndSeedAsync(ApplicationDbContext context)
    {
        try
        {
            var sql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserModulePermissions')
                BEGIN
                    CREATE TABLE [UserModulePermissions] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [UserId] INT NOT NULL,
                        [ModuleKey] NVARCHAR(100) NOT NULL,
                        [ModuleName] NVARCHAR(150) NOT NULL,
                        [ModuleCategory] NVARCHAR(100) NOT NULL DEFAULT '',
                        [RoutePath] NVARCHAR(200) NOT NULL DEFAULT '',
                        [IconClass] NVARCHAR(100) NOT NULL DEFAULT '',
                        [IsAllowed] BIT NOT NULL DEFAULT 1,
                        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        [UpdatedOn] DATETIME2 NULL,
                        [IsDeleted] BIT NOT NULL DEFAULT 0
                    );
                END";

            await context.Database.ExecuteSqlRawAsync(sql);

            // Seed default permissions for each user if not already present
            var users = await context.Users.Include(u => u.Role).ToListAsync();
            foreach (var user in users)
            {
                var roleName = user.Role?.Name?.ToLower() ?? "customer";
                var hasPermissions = await context.UserModulePermissions.AnyAsync(p => p.UserId == user.Id);
                
                if (!hasPermissions)
                {
                    foreach (var mod in SystemModules)
                    {
                        bool isAllowed = false;

                        if (roleName.Contains("superadmin"))
                        {
                            isAllowed = true; // SuperAdmin gets everything
                        }
                        else if (roleName.Contains("admin"))
                        {
                            // Admin gets all Admin + Masters + Customer
                            isAllowed = mod.Category == "Admin" || mod.Category == "SuperAdmin" || mod.Category == "Customer";
                        }
                        else if (roleName.Contains("delivery"))
                        {
                            // Rider gets delivery console + addresses
                            isAllowed = mod.Key == "delivery_console" || mod.Key == "addresses";
                        }
                        else
                        {
                            // Customer gets customer modules
                            isAllowed = mod.Category == "Customer";
                        }

                        context.UserModulePermissions.Add(new UserModulePermission
                        {
                            UserId = user.Id,
                            ModuleKey = mod.Key,
                            ModuleName = mod.Name,
                            ModuleCategory = mod.Category,
                            RoutePath = mod.Route,
                            IconClass = mod.Icon,
                            IsAllowed = isAllowed,
                            CreatedOn = DateTime.UtcNow
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModulePermissionTableSeeder] Error: {ex.Message}");
        }
    }
}
