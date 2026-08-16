using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Seed;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var requiredRoles = new List<(string Name, string Description)>
        {
            ("SuperAdmin", "Super Administrator with Complete Master Control"),
            ("Admin", "System Administrator"),
            ("Customer", "Application Customer"),
            ("DeliveryPartner", "Delivery Partner"),
            ("DeliveryBoy", "Delivery Partner"),
            ("RestaurantOwner", "Restaurant Owner / Partner")
        };

        foreach (var (name, desc) in requiredRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == name))
            {
                context.Roles.Add(new Role
                {
                    Name = name,
                    Description = desc
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
