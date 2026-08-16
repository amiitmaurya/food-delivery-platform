using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);

    Task<bool> ExistsAsync(string code);

    Task<List<Coupon>> GetActiveCouponsAsync();
}
 