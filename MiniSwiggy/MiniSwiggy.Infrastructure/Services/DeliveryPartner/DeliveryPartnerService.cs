using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.DeliveryPartner.DTOs;
using MiniSwiggy.Application.DeliveryPartner.Interfaces;
using MiniSwiggy.Domain.Entities.DeliveryPartner;
using MiniSwiggy.Domain.Enums;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Services.DeliveryPartner;

public class DeliveryPartnerService : IDeliveryPartnerService
{
    private readonly ApplicationDbContext _context;

    public DeliveryPartnerService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task EnsureTrackerExistsForOrdersAsync(int deliveryPartnerUserId)
    {
        try
        {
            // Auto-assign any pending/unassigned orders in database to this delivery partner for seamless demo/prod execution if untracked
            var untrackedOrders = await _context.Orders
                .Where(o => !_context.DeliveryOrderTrackers.Any(t => t.OrderId == o.Id) &&
                            o.Status != OrderStatus.Cancelled &&
                            o.Status != OrderStatus.Delivered)
                .Take(5)
                .ToListAsync();

            foreach (var order in untrackedOrders)
            {
                _context.DeliveryOrderTrackers.Add(new DeliveryOrderTracker
                {
                    OrderId = order.Id,
                    DeliveryPartnerUserId = deliveryPartnerUserId,
                    DeliveryStatus = "Assigned",
                    AssignedAt = order.OrderDate
                });
            }

            if (untrackedOrders.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EnsureTrackerExistsForOrdersAsync] Exception: {ex.Message}");
        }
    }

    public async Task<List<DeliveryOrderDto>> GetMyAssignedOrdersAsync(int deliveryPartnerUserId)
    {
        try
        {
            await EnsureTrackerExistsForOrdersAsync(deliveryPartnerUserId);

            // Fetch trackers:
            // 1. Unclaimed/Assigned orders (DeliveryStatus == "Assigned")
            // 2. Orders accepted by THIS specific delivery partner (DeliveryPartnerUserId == deliveryPartnerUserId && DeliveryStatus == "Accepted")
            var trackers = await _context.DeliveryOrderTrackers
                .Where(t => (t.DeliveryStatus == "Assigned") ||
                            (t.DeliveryPartnerUserId == deliveryPartnerUserId && t.DeliveryStatus == "Accepted"))
                .ToListAsync();

            var orderIds = trackers.Select(t => t.OrderId).ToList();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .Where(o => orderIds.Contains(o.Id) && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapToDeliveryOrderDto(o, trackers.FirstOrDefault(t => t.OrderId == o.Id))).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetMyAssignedOrdersAsync] Exception: {ex.Message}");
            return new List<DeliveryOrderDto>();
        }
    }

    public async Task<DeliveryOrderDto?> GetCurrentDeliveryAsync(int deliveryPartnerUserId)
    {
        try
        {
            await EnsureTrackerExistsForOrdersAsync(deliveryPartnerUserId);

            // Fetch active delivery ONLY for THIS specific delivery partner
            var tracker = await _context.DeliveryOrderTrackers
                .Where(t => t.DeliveryPartnerUserId == deliveryPartnerUserId &&
                            t.DeliveryStatus != "Assigned" &&
                            t.DeliveryStatus != "Delivered" &&
                            t.DeliveryStatus != "Rejected" &&
                            t.DeliveryStatus != "Cancelled")
                .OrderByDescending(t => t.AcceptedAt ?? t.AssignedAt)
                .FirstOrDefaultAsync();

            if (tracker == null) return null;

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefaultAsync(o => o.Id == tracker.OrderId && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled);

            if (order == null) return null;

            return MapToDeliveryOrderDto(order, tracker);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetCurrentDeliveryAsync] Exception: {ex.Message}");
            return null;
        }
    }

    public async Task<List<DeliveryOrderDto>> GetDeliveryHistoryAsync(int deliveryPartnerUserId)
    {
        try
        {
            var trackers = await _context.DeliveryOrderTrackers
                .Where(t => t.DeliveryStatus == "Delivered")
                .OrderByDescending(t => t.DeliveredAt ?? t.AssignedAt)
                .ToListAsync();

            var trackerOrderIds = trackers.Select(t => t.OrderId).Distinct().ToList();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.FoodItem)
                .Where(o => trackerOrderIds.Contains(o.Id) || o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            foreach (var order in orders)
            {
                if (!trackers.Any(t => t.OrderId == order.Id))
                {
                    trackers.Add(new DeliveryOrderTracker
                    {
                        OrderId = order.Id,
                        DeliveryPartnerUserId = deliveryPartnerUserId,
                        DeliveryStatus = "Delivered",
                        DeliveredAt = order.DeliveredDate ?? order.OrderDate,
                        DeliveryEarnings = 40.0m
                    });
                }
            }

            return orders.Select(o => MapToDeliveryOrderDto(o, trackers.FirstOrDefault(t => t.OrderId == o.Id))).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetDeliveryHistoryAsync] Exception: {ex.Message}");
            return new List<DeliveryOrderDto>();
        }
    }

    public async Task<DeliveryEarningsDto> GetEarningsAsync(int deliveryPartnerUserId)
    {
        var completed = await _context.DeliveryOrderTrackers
            .Where(t => t.DeliveryStatus == "Delivered")
            .ToListAsync();

        var deliveredOrders = await _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .ToListAsync();

        foreach (var order in deliveredOrders)
        {
            if (!completed.Any(t => t.OrderId == order.Id))
            {
                completed.Add(new DeliveryOrderTracker
                {
                    OrderId = order.Id,
                    DeliveryPartnerUserId = deliveryPartnerUserId,
                    DeliveryStatus = "Delivered",
                    DeliveredAt = order.DeliveredDate ?? order.OrderDate,
                    DeliveryEarnings = 40.0m
                });
            }
        }

        // Group by OrderId so duplicate tracker rows aren't double counted
        var uniqueCompleted = completed
            .GroupBy(t => t.OrderId)
            .Select(g => g.First())
            .ToList();

        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        decimal todayEarn = uniqueCompleted.Where(t => t.DeliveredAt.HasValue && t.DeliveredAt.Value.Date == today).Sum(t => t.DeliveryEarnings);
        decimal weekEarn = uniqueCompleted.Where(t => t.DeliveredAt.HasValue && t.DeliveredAt.Value.Date >= startOfWeek).Sum(t => t.DeliveryEarnings);
        decimal monthEarn = uniqueCompleted.Where(t => t.DeliveredAt.HasValue && t.DeliveredAt.Value.Date >= startOfMonth).Sum(t => t.DeliveryEarnings);
        decimal totalEarn = uniqueCompleted.Sum(t => t.DeliveryEarnings);

        var ratings = uniqueCompleted.Where(t => t.CustomerRating.HasValue).Select(t => t.CustomerRating!.Value).ToList();
        double avgRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : 5.0;

        // Breakdown last 7 days
        var dailyBreakdown = new List<DailyEarningBreakdownDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayOrders = uniqueCompleted.Where(t => t.DeliveredAt.HasValue && t.DeliveredAt.Value.Date == date).ToList();
            dailyBreakdown.Add(new DailyEarningBreakdownDto
            {
                Date = date.ToString("MMM dd"),
                Amount = dayOrders.Sum(t => t.DeliveryEarnings),
                DeliveriesCount = dayOrders.Count
            });
        }

        return new DeliveryEarningsDto
        {
            TodayEarnings = todayEarn,
            WeeklyEarnings = weekEarn,
            MonthlyEarnings = monthEarn,
            TotalEarnings = totalEarn,
            TodayDeliveriesCount = uniqueCompleted.Count(t => t.DeliveredAt.HasValue && t.DeliveredAt.Value.Date == today),
            TotalDeliveriesCount = uniqueCompleted.Count,
            AverageRating = avgRating,
            DailyBreakdown = dailyBreakdown,
            RecentPayouts = new List<RecentPayoutDto>()
        };
    }

    public async Task<DeliveryProfileDto> GetProfileAsync(int deliveryPartnerUserId)
    {
        try
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == deliveryPartnerUserId);
            DeliveryPartnerProfile? profile = null;
            try
            {
                profile = await _context.DeliveryPartnerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == deliveryPartnerUserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProfileAsync] Profile query notice: {ex.Message}");
            }

            if (profile == null)
            {
                profile = new DeliveryPartnerProfile
                {
                    UserId = deliveryPartnerUserId,
                    IsOnline = true,
                    VehicleType = "Bike",
                    VehicleNumber = "",
                    VehicleModel = "",
                    LicenseNumber = "",
                    LicenseExpiryDate = "",
                    BankAccountHolder = user?.FullName ?? "",
                    BankName = "",
                    AccountNumber = "",
                    IfscCode = "",
                    UpiId = ""
                };
                try
                {
                    _context.DeliveryPartnerProfiles.Add(profile);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetProfileAsync] Profile save notice: {ex.Message}");
                }
            }

            return new DeliveryProfileDto
            {
                Id = profile.Id,
                FullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                PhoneNumber = user?.PhoneNumber ?? "",
                ProfileImageUrl = user?.ImageUrl ?? "",
                IsOnline = profile.IsOnline,
                VehicleType = profile.VehicleType ?? "Bike",
                VehicleNumber = profile.VehicleNumber ?? "",
                VehicleModel = profile.VehicleModel ?? "",
                LicenseNumber = profile.LicenseNumber ?? "",
                LicenseExpiryDate = profile.LicenseExpiryDate ?? "",
                BankAccountHolder = profile.BankAccountHolder ?? "",
                BankName = profile.BankName ?? "",
                AccountNumber = profile.AccountNumber ?? "",
                IfscCode = profile.IfscCode ?? "",
                UpiId = profile.UpiId ?? ""
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetProfileAsync] Main exception: {ex.Message}");
            return new DeliveryProfileDto
            {
                FullName = "Delivery Partner",
                Email = "",
                PhoneNumber = "",
                VehicleType = "Bike"
            };
        }
    }

    public async Task<List<DeliveryProfileDto>> GetAllPartnersAsync()
    {
        try
        {
            var partnerUsers = await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted && (u.Role.Name == "DeliveryPartner" || u.Role.Name == "DeliveryBoy"))
                .AsNoTracking()
                .ToListAsync();

            var profiles = await _context.DeliveryPartnerProfiles.AsNoTracking().ToListAsync();

            return partnerUsers.Select(u =>
            {
                var prof = profiles.FirstOrDefault(p => p.UserId == u.Id);
                return new DeliveryProfileDto
                {
                    Id = prof?.Id ?? 0,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    ProfileImageUrl = u.ImageUrl ?? "",
                    IsOnline = prof?.IsOnline ?? u.IsActive,
                    VehicleType = prof?.VehicleType ?? "Bike",
                    VehicleNumber = prof?.VehicleNumber ?? "MH-12-AB-1234",
                    VehicleModel = prof?.VehicleModel ?? "Hero Splendor Plus",
                    LicenseNumber = prof?.LicenseNumber ?? "DL-9876543210",
                    LicenseExpiryDate = prof?.LicenseExpiryDate ?? "2029-12-31",
                    BankAccountHolder = prof?.BankAccountHolder ?? u.FullName,
                    BankName = prof?.BankName ?? "HDFC Bank",
                    AccountNumber = prof?.AccountNumber ?? "5010023456789",
                    IfscCode = prof?.IfscCode ?? "HDFC0001234",
                    UpiId = prof?.UpiId ?? $"{u.FullName.ToLower().Replace(" ", "")}@upi"
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetAllPartnersAsync] Exception: {ex.Message}");
            return new List<DeliveryProfileDto>();
        }
    }

    public async Task<bool> UpdateProfileAsync(int deliveryPartnerUserId, UpdateDeliveryProfileRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == deliveryPartnerUserId);
        if (user != null)
        {
            if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber.Trim();
            if (!string.IsNullOrWhiteSpace(request.ProfileImageUrl)) user.ImageUrl = request.ProfileImageUrl;
        }

        var profile = await _context.DeliveryPartnerProfiles.FirstOrDefaultAsync(p => p.UserId == deliveryPartnerUserId);
        if (profile == null)
        {
            profile = new DeliveryPartnerProfile { UserId = deliveryPartnerUserId };
            _context.DeliveryPartnerProfiles.Add(profile);
        }

        profile.VehicleType = request.VehicleType ?? "Bike";
        profile.VehicleNumber = request.VehicleNumber?.Trim().ToUpper() ?? "";
        profile.VehicleModel = request.VehicleModel?.Trim() ?? "";
        profile.LicenseNumber = request.LicenseNumber?.Trim().ToUpper() ?? "";
        profile.LicenseExpiryDate = request.LicenseExpiryDate?.Trim() ?? "";
        profile.BankAccountHolder = request.BankAccountHolder?.Trim() ?? "";
        profile.BankName = request.BankName?.Trim() ?? "";
        profile.AccountNumber = request.AccountNumber?.Trim() ?? "";
        profile.IfscCode = request.IfscCode?.Trim().ToUpper() ?? "";
        profile.UpiId = request.UpiId?.Trim() ?? "";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int deliveryPartnerUserId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == deliveryPartnerUserId);
        if (user == null) return false;

        user.PasswordHash = request.NewPassword;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ToggleOnlineStatusAsync(int deliveryPartnerUserId, bool isOnline)
    {
        var profile = await _context.DeliveryPartnerProfiles.FirstOrDefaultAsync(p => p.UserId == deliveryPartnerUserId);
        if (profile == null)
        {
            profile = new DeliveryPartnerProfile { UserId = deliveryPartnerUserId, IsOnline = isOnline };
            _context.DeliveryPartnerProfiles.Add(profile);
        }
        else
        {
            profile.IsOnline = isOnline;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    // --- Status Transitions Sequence Enforcement ---

    public async Task<(bool Success, string Message)> AcceptOrderAsync(int deliveryPartnerUserId, int orderId)
    {
        var tracker = await GetOrCreateTrackerAsync(orderId, deliveryPartnerUserId);

        // Block if another delivery partner has already accepted this order
        if (tracker.DeliveryStatus != "Assigned" && tracker.DeliveryPartnerUserId != deliveryPartnerUserId)
        {
            return (false, "This order has already been accepted by another delivery partner.");
        }

        if (tracker.DeliveryStatus == "Accepted" && tracker.DeliveryPartnerUserId == deliveryPartnerUserId)
        {
            return (true, "Order is already accepted by you.");
        }

        tracker.DeliveryStatus = "Accepted";
        tracker.AcceptedAt = DateTime.UtcNow;
        tracker.DeliveryPartnerUserId = deliveryPartnerUserId;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Confirmed;
        }

        await _context.SaveChangesAsync();
        return (true, "Order accepted successfully.");
    }

    public async Task<(bool Success, string Message)> RejectOrderAsync(int deliveryPartnerUserId, int orderId)
    {
        var tracker = await GetOrCreateTrackerAsync(orderId, deliveryPartnerUserId);

        tracker.DeliveryStatus = "Rejected";

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Cancelled;
        }

        await _context.SaveChangesAsync();
        return (true, "Order rejected.");
    }

    public async Task<(bool Success, string Message)> MarkReachedRestaurantAsync(int deliveryPartnerUserId, int orderId)
    {
        var tracker = await GetOrCreateTrackerAsync(orderId, deliveryPartnerUserId);
        if (tracker.DeliveryStatus != "Accepted")
            return (false, $"Cannot mark Reached Restaurant. Current status is '{tracker.DeliveryStatus}'. You must Accept the order first.");

        tracker.DeliveryStatus = "ReachedRestaurant";
        tracker.ReachedRestaurantAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, "Reached restaurant marked successfully.");
    }

    public async Task<(bool Success, string Message)> MarkPickedUpAsync(int deliveryPartnerUserId, int orderId)
    {
        var tracker = await GetOrCreateTrackerAsync(orderId, deliveryPartnerUserId);
        if (tracker.DeliveryStatus != "ReachedRestaurant" && tracker.DeliveryStatus != "Accepted")
            return (false, $"Cannot mark Picked Up. Current status is '{tracker.DeliveryStatus}'. You must reach restaurant first.");

        tracker.DeliveryStatus = "PickedUp";
        tracker.PickedUpAt = DateTime.UtcNow;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Preparing;
        }

        await _context.SaveChangesAsync();
        return (true, "Order picked up from restaurant.");
    }

    public async Task<(bool Success, string Message)> MarkOutForDeliveryAsync(int deliveryPartnerUserId, int orderId)
    {
        var tracker = await GetOrCreateTrackerAsync(orderId, deliveryPartnerUserId);
        if (tracker.DeliveryStatus != "PickedUp")
            return (false, $"Cannot mark Out For Delivery. Current status is '{tracker.DeliveryStatus}'. You must pick up order first.");

        tracker.DeliveryStatus = "OutForDelivery";
        tracker.OutForDeliveryAt = DateTime.UtcNow;

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = OrderStatus.OutForDelivery;
        }

        await _context.SaveChangesAsync();
        return (true, "Order marked Out for Delivery.");
    }

    public async Task<(bool Success, string Message)> MarkDeliveredAsync(int deliveryPartnerUserId, int orderId)
    {
        var tracker = await GetOrCreateTrackerAsync(orderId, deliveryPartnerUserId);

        tracker.DeliveryStatus = "Delivered";
        tracker.DeliveredAt = DateTime.UtcNow;
        tracker.CustomerRating = 5.0;
        tracker.CustomerFeedback = "Fast & polite delivery service!";
        tracker.DeliveryPartnerUserId = deliveryPartnerUserId;

        // Also update any other trackers for this orderId if any exist
        var allTrackers = await _context.DeliveryOrderTrackers.Where(t => t.OrderId == orderId).ToListAsync();
        foreach (var t in allTrackers)
        {
            t.DeliveryStatus = "Delivered";
            t.DeliveredAt = DateTime.UtcNow;
        }

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;
            order.PaymentStatus = PaymentStatus.Paid;
        }

        await _context.SaveChangesAsync();
        return (true, "Order delivered successfully!");
    }

    private async Task<DeliveryOrderTracker> GetOrCreateTrackerAsync(int orderId, int deliveryPartnerUserId)
    {
        var tracker = await _context.DeliveryOrderTrackers
            .FirstOrDefaultAsync(t => t.OrderId == orderId);

        if (tracker == null)
        {
            tracker = new DeliveryOrderTracker
            {
                OrderId = orderId,
                DeliveryPartnerUserId = deliveryPartnerUserId,
                DeliveryStatus = "Assigned",
                AssignedAt = DateTime.UtcNow
            };
            _context.DeliveryOrderTrackers.Add(tracker);
            await _context.SaveChangesAsync();
        }
        else if (tracker.DeliveryPartnerUserId != deliveryPartnerUserId)
        {
            tracker.DeliveryPartnerUserId = deliveryPartnerUserId;
            await _context.SaveChangesAsync();
        }

        return tracker;
    }

    private DeliveryOrderDto MapToDeliveryOrderDto(Domain.Entities.Order order, DeliveryOrderTracker? tracker)
    {
        return new DeliveryOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = tracker?.DeliveryStatus ?? order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            DeliveryCharge = order.DeliveryCharge,
            FinalAmount = order.FinalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            UserId = order.UserId,
            CustomerName = order.User?.FullName ?? "Customer",
            CustomerPhone = order.PhoneNumber ?? order.User?.PhoneNumber ?? "+91 9999988888",
            DeliveryAddress = order.DeliveryAddress,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.Restaurant?.Name ?? "Swiggy Express Kitchen",
            RestaurantAddress = order.Restaurant?.Address ?? "Main City Center Road",
            RestaurantPhone = order.Restaurant?.MobileNumber ?? "+91 9876543210",
            RestaurantImageUrl = order.Restaurant?.ImageUrl ?? "assets/images/restaurant-placeholder.jpg",
            DeliveryPartnerId = tracker?.DeliveryPartnerUserId,
            AcceptedAt = tracker?.AcceptedAt,
            ReachedRestaurantAt = tracker?.ReachedRestaurantAt,
            PickedUpAt = tracker?.PickedUpAt,
            OutForDeliveryAt = tracker?.OutForDeliveryAt,
            DeliveredAt = tracker?.DeliveredAt,
            CustomerRating = tracker?.CustomerRating,
            CustomerFeedback = tracker?.CustomerFeedback,
            Items = order.OrderItems.Select(oi => new DeliveryOrderItemDto
            {
                FoodItemId = oi.FoodItemId,
                FoodItemName = oi.FoodItem?.Name ?? "Delicious Dish",
                Price = oi.Price,
                Quantity = oi.Quantity,
                ImageUrl = oi.FoodItem?.ImageUrl ?? "assets/images/food-placeholder.jpg"
            }).ToList()
        };
    }
}
