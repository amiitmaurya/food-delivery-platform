using MiniSwiggy.Domain.Common;
using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Payment : BaseEntity
{
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }

    // Razorpay / Stripe / CashOnDelivery / UPI
    public PaymentMethod PaymentMethod { get; set; }

    // Pending / Success / Failed / Refunded
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? TransactionId { get; set; }

    public string? GatewayOrderId { get; set; }

    public DateTime? PaidOn { get; set; }
}
