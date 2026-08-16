using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Payment;

public class PaymentResponse
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public int UserId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string PaymentMethodName => PaymentMethod.ToString();

    public PaymentStatus PaymentStatus { get; set; }

    public string PaymentStatusName => PaymentStatus.ToString();

    public string? TransactionId { get; set; }

    public string? GatewayOrderId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? PaidOn { get; set; }
}