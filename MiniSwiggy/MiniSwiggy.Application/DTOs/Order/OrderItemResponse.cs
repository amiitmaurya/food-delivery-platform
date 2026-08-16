using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Order;

public class OrderItemResponse
{
    public int FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
    public string ? DeliveryAddress { get; set; }

    public string ? RestaurantName { get; set; }

    public List<OrderItemResponse> Items { get; set; } = [];
}
