using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(x =>
                x.OrderId == orderId &&
                !x.IsDeleted);
    }

    public async Task<List<Payment>> GetByUserPaymentsAsync(int userId)
    {
        return await _context.Payments
            .Include(x => x.Order)
                .ThenInclude(o => o.User)
            .Where(x =>
                x.Order != null &&
                x.Order.UserId == userId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetAllWithDetailsAsync()
    {
        return await _context.Payments
            .Include(x => x.Order)
                .ThenInclude(o => o.User)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
    }
}
