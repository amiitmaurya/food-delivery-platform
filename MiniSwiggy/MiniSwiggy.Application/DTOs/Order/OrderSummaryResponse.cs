using MiniSwiggy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Order;

public class OrderSummaryResponse
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }

    public decimal FinalAmount { get; set; }

    public string DeliveryAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public DateTime OrderDate { get; set; }

    public List<OrderItemResponse> Items { get; set; } = [];
}
