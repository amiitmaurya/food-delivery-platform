using MiniSwiggy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var cleanCode = code.Trim().ToUpper();

        return await _context.Coupons
            .FirstOrDefaultAsync(x =>
                x.Code.ToUpper() == cleanCode &&
                !x.IsDeleted);
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.Coupons
            .AnyAsync(x =>
                x.Code == code &&
                !x.IsDeleted);
    }

    public async Task<List<Coupon>> GetActiveCouponsAsync()
    {
        var today = DateTime.UtcNow;

        return await _context.Coupons
            .Where(x =>
                !x.IsDeleted &&
                x.IsActive &&
                x.StartDate <= today &&
                x.ExpiryDate >= today)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }
}
