using MiniSwiggy.Application.DTOs.Order;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CancelOrderAsync(int orderId, int userId)
    {
        // Get Order
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        if (order == null)
            return false;

        // Check Order Owner
        if (order.UserId != userId)
            return false;

        // Already Delivered
        if (order.Status == OrderStatus.Delivered)
            return false;

        // Already Cancelled
        if (order.Status == OrderStatus.Cancelled)
            return false;

        // Cancel Order
        order.Status = OrderStatus.Cancelled;

        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId, int userId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(orderId);

        if (order == null)
            return null;

        // Security Check
        if (order.UserId != userId)
            return null;

        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserName = order.User?.FullName ?? "",
            UserEmail = order.User?.Email ?? "",
            UserPhone = !string.IsNullOrEmpty(order.PhoneNumber) ? order.PhoneNumber : (order.User?.PhoneNumber ?? ""),
            TotalAmount = order.TotalAmount,
            DeliveryCharge = order.DeliveryCharge,
            Discount = order.Discount,
            Tax = order.Tax,
            FinalAmount = order.FinalAmount,
            DeliveryAddress = order.DeliveryAddress,
            PhoneNumber = order.PhoneNumber,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc),

            Items = order.OrderItems.Select(x => new OrderItemResponse
            {
                FoodItemId = x.FoodItemId,
                FoodName = x.FoodName,
                Price = x.Price,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderSummaryResponse>> GetMyOrdersAsync(int userId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(userId);

        return orders.Select(x => new OrderSummaryResponse
        {
            Id = x.Id,
            OrderNumber = x.OrderNumber,
            UserId = x.UserId,
            UserName = !string.IsNullOrWhiteSpace(x.User?.FullName) ? x.User.FullName : (!string.IsNullOrWhiteSpace(x.User?.Email) ? x.User.Email : "Customer"),
            UserEmail = x.User?.Email ?? "",
            UserPhone = !string.IsNullOrEmpty(x.PhoneNumber) ? x.PhoneNumber : (x.User?.PhoneNumber ?? ""),
            FinalAmount = x.FinalAmount,

            TotalAmount = x.TotalAmount,
            DeliveryCharge = x.DeliveryCharge,
            Discount = x.Discount,
            Tax = x.Tax,

            DeliveryAddress = x.DeliveryAddress,
            PhoneNumber = x.PhoneNumber,
            RestaurantName = x.Restaurant?.Name ?? "",

            Status = x.Status,
            PaymentStatus = x.PaymentStatus,
            OrderDate = DateTime.SpecifyKind(x.OrderDate, DateTimeKind.Utc),

            Items = x.OrderItems.Select(i => new OrderItemResponse
            {
                FoodItemId = i.FoodItemId,
                FoodName = i.FoodName,
                Price = i.Price,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList()

        }).ToList();
    }

    public async Task<IEnumerable<OrderSummaryResponse>> GetAllOrdersAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllOrdersWithItemsAsync();

        return orders.Select(x => new OrderSummaryResponse
        {
            Id = x.Id,
            OrderNumber = x.OrderNumber,
            UserId = x.UserId,
            UserName = !string.IsNullOrWhiteSpace(x.User?.FullName) ? x.User.FullName : (!string.IsNullOrWhiteSpace(x.User?.Email) ? x.User.Email : "Customer"),
            UserEmail = x.User?.Email ?? "",
            UserPhone = !string.IsNullOrEmpty(x.PhoneNumber) ? x.PhoneNumber : (x.User?.PhoneNumber ?? ""),
            FinalAmount = x.FinalAmount,
            TotalAmount = x.TotalAmount,
            DeliveryCharge = x.DeliveryCharge,
            Discount = x.Discount,
            Tax = x.Tax,
            DeliveryAddress = x.DeliveryAddress,
            PhoneNumber = x.PhoneNumber,
            RestaurantName = x.Restaurant?.Name ?? "",
            Status = x.Status,
            PaymentStatus = x.PaymentStatus,
            OrderDate = DateTime.SpecifyKind(x.OrderDate, DateTimeKind.Utc),
            Items = x.OrderItems != null ? x.OrderItems.Select(i => new OrderItemResponse
            {
                FoodItemId = i.FoodItemId,
                FoodName = i.FoodName,
                Price = i.Price,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice
            }).ToList() : new List<OrderItemResponse>()
        }).ToList();
    }

    public async Task<OrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request)
    {
        // 1. Get User Cart
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);

        if (cart == null || !cart.CartItems.Any())
            throw new Exception("Cart is empty.");

        var restaurantId = cart.CartItems.First().FoodItem.Category.RestaurantId;

        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);

        if (restaurant == null)
        {
            throw new Exception("Restaurant not found.");
        }

        decimal appliedDiscount = 0;
        if (request.DiscountAmount.HasValue && request.DiscountAmount.Value > 0)
        {
            appliedDiscount = request.DiscountAmount.Value;
        }
        else if (restaurant.Discount > 0)
        {
            appliedDiscount = restaurant.Discount;
        }

        // 2. Create Order
        var order = new Order
        {
            UserId = userId,
            RestaurantId = cart.CartItems.First().FoodItem.Category.RestaurantId,
            OrderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}",
            TotalAmount = cart.TotalAmount,
            DeliveryCharge = restaurant.DeliveryCharge,
            Discount = appliedDiscount,
            Tax = Math.Round(cart.TotalAmount * 0.05m, 2),
            DeliveryAddress = request.DeliveryAddress,
            PhoneNumber = request.PhoneNumber,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = (request.PaymentMethod == PaymentMethod.CashOnDelivery) ? PaymentStatus.Pending : PaymentStatus.Paid,
            Status = OrderStatus.Pending,
            OrderDate = DateTime.UtcNow
        };

        order.FinalAmount =
            Math.Max(0, Math.Round(order.TotalAmount +
            order.DeliveryCharge +
            order.Tax -
            order.Discount, 2));

        // 3. Copy Cart Items To Order Items
        foreach (var item in cart.CartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                FoodItemId = item.FoodItemId,
                FoodName = item.FoodItem.Name,
                Price = item.UnitPrice,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            });
        }

        // 4. Save Order
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // 4a. Update Coupon Usage Limit if applied
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(request.CouponCode);
            if (coupon != null)
            {
                coupon.UsedCount += 1;
                coupon.UpdatedOn = DateTime.UtcNow;
                _unitOfWork.Coupons.Update(coupon);
            }
        }

        // 4b. Record Payment in Payments table (for both COD and Online / UPI / Card)
        var isOnlinePaid = order.PaymentMethod != PaymentMethod.CashOnDelivery;
        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.FinalAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = isOnlinePaid ? PaymentStatus.Paid : PaymentStatus.Pending,
            TransactionId = isOnlinePaid ? $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(100, 999)}" : null,
            CreatedOn = DateTime.UtcNow,
            PaidOn = isOnlinePaid ? DateTime.UtcNow : null
        };
        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // 5. Clear Cart
        foreach (var item in cart.CartItems)
        {
            _unitOfWork.CartItems.Delete(item);
        }

        cart.TotalAmount = 0;

        _unitOfWork.Carts.Update(cart);

        await _unitOfWork.SaveChangesAsync();

        // 6. Return Response
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            DeliveryCharge = order.DeliveryCharge,
            Discount = order.Discount,
            Tax = order.Tax,
            FinalAmount = order.FinalAmount,
            DeliveryAddress = order.DeliveryAddress,
            PhoneNumber = order.PhoneNumber,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc),

            Items = order.OrderItems.Select(x => new OrderItemResponse
            {
                FoodItemId = x.FoodItemId,
                FoodName = x.FoodName,
                Price = x.Price,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice
            }).ToList()
        };
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        if (order == null)
            return false;

        order.Status = request.Status;

        // Auto update delivery date and sync payment for COD
        if (request.Status == OrderStatus.Delivered)
        {
            order.DeliveredDate = DateTime.UtcNow;

            if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                order.PaymentStatus = PaymentStatus.Paid;

                var payment = await _unitOfWork.Payments.GetByOrderIdAsync(order.Id);
                if (payment != null)
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.PaidOn = DateTime.UtcNow;
                    payment.UpdatedOn = DateTime.UtcNow;
                    _unitOfWork.Payments.Update(payment);
                }
            }
        }

        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
