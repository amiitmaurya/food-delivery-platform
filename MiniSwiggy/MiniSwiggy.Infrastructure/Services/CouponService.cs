using MiniSwiggy.Application.DTOs.Coupon;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;

    public CouponService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CouponResponse>> GetAllAsync()
    {
        var coupons = await _unitOfWork.Coupons.GetAllAsync();

        return coupons
            .Where(x => !x.IsDeleted)
            .Select(x => new CouponResponse
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                DiscountType = x.DiscountType,
                DiscountValue = x.DiscountValue,
                MinimumOrderAmount = x.MinimumOrderAmount,
                MaximumDiscount = x.MaximumDiscount,
                StartDate = x.StartDate,
                ExpiryDate = x.ExpiryDate,
                UsageLimit = x.UsageLimit,
                UsedCount = x.UsedCount,
                IsActive = x.IsActive
            })
            .ToList();
    }

    public async Task<CouponResponse?> GetByIdAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);

        if (coupon == null || coupon.IsDeleted)
            return null;

        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Description = coupon.Description,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            MinimumOrderAmount = coupon.MinimumOrderAmount,
            MaximumDiscount = coupon.MaximumDiscount,
            StartDate = coupon.StartDate,
            ExpiryDate = coupon.ExpiryDate,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            IsActive = coupon.IsActive
        };
    }

    public async Task<bool> CreateAsync(CreateCouponRequest request)
    {
        if (await _unitOfWork.Coupons.ExistsAsync(request.Code))
            return false;

        var coupon = new Coupon
        {
            Code = request.Code,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaximumDiscount = request.MaximumDiscount,
            StartDate = request.StartDate,
            ExpiryDate = request.ExpiryDate,
            UsageLimit = request.UsageLimit,
            IsActive = request.IsActive
        };

        await _unitOfWork.Coupons.AddAsync(coupon);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(UpdateCouponRequest request)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(request.Id);

        if (coupon == null || coupon.IsDeleted)
            return false;

        coupon.Code = request.Code;
        coupon.Description = request.Description;
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        coupon.MaximumDiscount = request.MaximumDiscount;
        coupon.StartDate = request.StartDate;
        coupon.ExpiryDate = request.ExpiryDate;
        coupon.UsageLimit = request.UsageLimit;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Coupons.Update(coupon);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);

        if (coupon == null)
            return false;

        try
        {
            _unitOfWork.Coupons.Delete(coupon);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new BadRequestException("Cannot delete coupon because it is referenced by existing records.");
        }
    }

    public async Task<ApplyCouponResponse> ApplyCouponAsync(ApplyCouponRequest request)
    {
        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(request.CouponCode);

        if (coupon == null || coupon.IsDeleted)
        {
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Invalid coupon."
            };
        }

        if (!coupon.IsActive)
        {
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Coupon is inactive."
            };
        }

        var now = DateTime.UtcNow;

        if (now.Date < coupon.StartDate.Date)
        {
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Coupon is not active yet."
            };
        }

        // Expiry check allows full expiry day until 23:59:59
        if (now.Date > coupon.ExpiryDate.Date)
        {
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Coupon has expired."
            };
        }

        if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
        {
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Coupon usage limit reached."
            };
        }

        if (coupon.MinimumOrderAmount > 0 && request.CartTotal < coupon.MinimumOrderAmount)
        {
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = $"Minimum order amount of ₹{coupon.MinimumOrderAmount} required for coupon '{coupon.Code}'."
            };
        }

        decimal discount = 0;

        if (string.Equals(coupon.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase) || coupon.DiscountValue <= 100)
        {
            discount = request.CartTotal * coupon.DiscountValue / 100;

            if (coupon.MaximumDiscount.HasValue && coupon.MaximumDiscount.Value > 0 && discount > coupon.MaximumDiscount.Value)
            {
                discount = coupon.MaximumDiscount.Value;
            }
        }
        else
        {
            discount = coupon.DiscountValue;
        }

        discount = Math.Round(discount, 2);

        return new ApplyCouponResponse
        {
            IsValid = true,
            Message = $"🎉 Coupon '{coupon.Code}' applied successfully! Saved ₹{discount}.",
            OriginalAmount = request.CartTotal,
            Discount = discount,
            DiscountAmount = discount,
            FinalAmount = Math.Max(0, request.CartTotal - discount)
        };
    }
}
