using System;
using System.Collections.Generic;

namespace MiniSwiggy.Application.DeliveryPartner.DTOs;

public class DeliveryOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal FinalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    // Customer details
    public int UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    // Restaurant details
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantAddress { get; set; } = string.Empty;
    public string RestaurantPhone { get; set; } = string.Empty;
    public string RestaurantImageUrl { get; set; } = string.Empty;

    // Assigned delivery partner details
    public int? DeliveryPartnerId { get; set; }
    public string? DeliveryPartnerName { get; set; }

    // Delivery specific metadata
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ReachedRestaurantAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? OutForDeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public double? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }

    public List<DeliveryOrderItemDto> Items { get; set; } = new();
}

public class DeliveryOrderItemDto
{
    public int FoodItemId { get; set; }
    public string FoodItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
