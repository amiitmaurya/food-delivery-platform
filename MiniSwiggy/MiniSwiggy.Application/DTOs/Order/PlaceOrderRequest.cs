using MiniSwiggy.Domain.Enums;
using System;

namespace MiniSwiggy.Application.DTOs.Order;

public class PlaceOrderRequest
{
    public string DeliveryAddress { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; }

    public string? CouponCode { get; set; }

    public decimal? DiscountAmount { get; set; }
}