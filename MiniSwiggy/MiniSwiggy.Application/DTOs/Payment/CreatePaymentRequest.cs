using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Payment;

public class CreatePaymentRequest
{
    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
}
