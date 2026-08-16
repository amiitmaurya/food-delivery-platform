using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Payment;

public class UpdatePaymentStatusRequest
{
    public int PaymentId { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string? TransactionId { get; set; }

    public string? GatewayOrderId { get; set; }
}
