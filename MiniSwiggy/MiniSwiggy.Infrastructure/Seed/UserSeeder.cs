using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Entities.DeliveryPartner;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Seed;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var deliveryRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "DeliveryBoy" || r.Name == "DeliveryPartner");
        if (deliveryRole == null)
        {
            deliveryRole = new Role { Name = "DeliveryPartner", Description = "Delivery Partner Role" };
            context.Roles.Add(deliveryRole);
            await context.SaveChangesAsync();
        }

        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");

        // Seed Super Admin User
        if (superAdminRole != null && !await context.Users.AnyAsync(u => u.Email == "superadmin@miniswiggy.com"))
        {
            context.Users.Add(new User
            {
                FullName = "Master Super Administrator",
                Email = "superadmin@miniswiggy.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                PhoneNumber = "+91 9000000000",
                IsActive = true,
                EmailVerified = true,
                RoleId = superAdminRole.Id
            });
            await context.SaveChangesAsync();
        }

        // Seed Delivery Partner User
        var deliveryUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "delivery@miniswiggy.com");
        if (deliveryUser == null)
        {
            deliveryUser = new User
            {
                FullName = "Vikram Singh (Delivery Partner)",
                Email = "delivery@miniswiggy.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                PhoneNumber = "+91 9876543210",
                IsActive = true,
                EmailVerified = true,
                RoleId = deliveryRole.Id,
                ImageUrl = "assets/images/delivery-avatar.png"
            };
            context.Users.Add(deliveryUser);
            await context.SaveChangesAsync();

            // Seed Delivery Profile
            context.DeliveryPartnerProfiles.Add(new DeliveryPartnerProfile
            {
                UserId = deliveryUser.Id,
                IsOnline = true,
                VehicleType = "Bike",
                VehicleNumber = "MH-12-AB-1234",
                VehicleModel = "Hero Splendor Plus",
                LicenseNumber = "DL-9876543210123",
                LicenseExpiryDate = "2029-12-31",
                BankAccountHolder = "Vikram Singh",
                BankName = "HDFC Bank",
                AccountNumber = "5010023456789",
                IfscCode = "HDFC0001234",
                UpiId = "vikram@upi",
                Rating = 4.9,
                TotalRatingsCount = 15
            });

            await context.SaveChangesAsync();
        }

        // Seed Admin User
        if (adminRole != null && !await context.Users.AnyAsync(u => u.Email == "admin@miniswiggy.com"))
        {
            context.Users.Add(new User
            {
                FullName = "System Administrator",
                Email = "admin@miniswiggy.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                PhoneNumber = "+91 9999999999",
                IsActive = true,
                EmailVerified = true,
                RoleId = adminRole.Id
            });
            await context.SaveChangesAsync();
        }

        // Seed Customer User
        if (customerRole != null && !await context.Users.AnyAsync(u => u.Email == "customer@miniswiggy.com"))
        {
            context.Users.Add(new User
            {
                FullName = "Amit Sharma (Customer)",
                Email = "customer@miniswiggy.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                PhoneNumber = "+91 9888888888",
                IsActive = true,
                EmailVerified = true,
                RoleId = customerRole.Id
            });
            await context.SaveChangesAsync();
        }

        // Seed Sample Orders if database has 0 orders
        if (!await context.Orders.AnyAsync())
        {
            var customer = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@miniswiggy.com");
            var restaurant = await context.Restaurants.FirstOrDefaultAsync();
            var foodItem = await context.FoodItems.FirstOrDefaultAsync();

            if (customer != null && restaurant != null && foodItem != null)
            {
                var order1 = new Order
                {
                    UserId = customer.Id,
                    RestaurantId = restaurant.Id,
                    OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-1001",
                    TotalAmount = foodItem.Price * 2,
                    DeliveryCharge = 30,
                    Tax = 15,
                    Discount = 0,
                    FinalAmount = (foodItem.Price * 2) + 45,
                    DeliveryAddress = "Flat 402, Lotus Heights, Sector 18, Noida",
                    PhoneNumber = customer.PhoneNumber,
                    Status = MiniSwiggy.Domain.Enums.OrderStatus.Delivered,
                    PaymentMethod = MiniSwiggy.Domain.Enums.PaymentMethod.UPI,
                    PaymentStatus = MiniSwiggy.Domain.Enums.PaymentStatus.Paid,
                    OrderDate = DateTime.UtcNow.AddHours(-3),
                    DeliveredDate = DateTime.UtcNow.AddHours(-2)
                };
                context.Orders.Add(order1);
                await context.SaveChangesAsync();

                context.OrderItems.Add(new OrderItem
                {
                    OrderId = order1.Id,
                    FoodItemId = foodItem.Id,
                    FoodName = foodItem.Name,
                    Price = foodItem.Price,
                    Quantity = 2,
                    TotalPrice = foodItem.Price * 2
                });

                var order2 = new Order
                {
                    UserId = customer.Id,
                    RestaurantId = restaurant.Id,
                    OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-1002",
                    TotalAmount = foodItem.Price,
                    DeliveryCharge = 30,
                    Tax = 10,
                    Discount = 20,
                    FinalAmount = foodItem.Price + 20,
                    DeliveryAddress = "Flat 402, Lotus Heights, Sector 18, Noida",
                    PhoneNumber = customer.PhoneNumber,
                    Status = MiniSwiggy.Domain.Enums.OrderStatus.Confirmed,
                    PaymentMethod = MiniSwiggy.Domain.Enums.PaymentMethod.UPI,
                    PaymentStatus = MiniSwiggy.Domain.Enums.PaymentStatus.Paid,
                    OrderDate = DateTime.UtcNow.AddMinutes(-35)
                };
                context.Orders.Add(order2);
                await context.SaveChangesAsync();

                context.OrderItems.Add(new OrderItem
                {
                    OrderId = order2.Id,
                    FoodItemId = foodItem.Id,
                    FoodName = foodItem.Name,
                    Price = foodItem.Price,
                    Quantity = 1,
                    TotalPrice = foodItem.Price
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
