using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Coupon;

public class UpdateCouponRequest
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = "Flat";

    public decimal DiscountValue { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    public decimal? MaximumDiscount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int UsageLimit { get; set; }

    public bool IsActive { get; set; }
}
