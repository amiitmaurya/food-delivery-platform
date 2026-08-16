using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId);
    Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync();

    Task<Order?> GetOrderDetailsAsync(int orderId);
    Task<Order?> GetByIdWithItemsAsync(int orderId);

    Task<Order?> GetByOrderNumberAsync(string orderNumber);
}
