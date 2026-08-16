using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Coupon;

public class ApplyCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;

    public decimal CartTotal { get; set; }
}