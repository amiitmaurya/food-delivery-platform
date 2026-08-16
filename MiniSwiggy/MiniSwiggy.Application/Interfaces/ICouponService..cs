using MiniSwiggy.Application.DTOs.Coupon;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface ICouponService
{
    Task<List<CouponResponse>> GetAllAsync();

    Task<CouponResponse?> GetByIdAsync(int id);

    Task<bool> CreateAsync(CreateCouponRequest request);

    Task<bool> UpdateAsync(UpdateCouponRequest request);

    Task<bool> DeleteAsync(int id);

    Task<ApplyCouponResponse> ApplyCouponAsync(ApplyCouponRequest request);
}
