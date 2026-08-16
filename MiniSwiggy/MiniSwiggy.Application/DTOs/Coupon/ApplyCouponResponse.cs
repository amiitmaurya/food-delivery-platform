using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Coupon;

public class ApplyCouponResponse
{
    public bool IsValid { get; set; }

    public string Message { get; set; } = string.Empty;

    public decimal OriginalAmount { get; set; }

    public decimal Discount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }
}
