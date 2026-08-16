using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Flat / Percentage
    public string DiscountType { get; set; } = "Flat";

    public decimal DiscountValue { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    public decimal? MaximumDiscount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public bool IsActive { get; set; } = true;
}
