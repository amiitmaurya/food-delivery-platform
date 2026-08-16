using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId && !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync()
    {
        return await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Where(o => !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderDetailsAsync(int orderId)
    {
        return await _context.Orders
            .Include(x => x.User)
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDeleted);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber && !x.IsDeleted);
    }

    public async Task<Order?> GetByIdWithItemsAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
    }

}