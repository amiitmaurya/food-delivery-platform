using MiniSwiggy.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(int userId, PlaceOrderRequest request);

    Task<IEnumerable<OrderSummaryResponse>> GetMyOrdersAsync(int userId);

    Task<IEnumerable<OrderSummaryResponse>> GetAllOrdersAsync();

    Task<OrderResponse?> GetOrderByIdAsync(int orderId, int userId);

    Task<bool> CancelOrderAsync(int orderId, int userId);

    Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);
}
