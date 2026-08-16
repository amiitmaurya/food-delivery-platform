using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Cart;

public class CartItemResponse
{
    public int Id { get; set; }

    public int FoodItemId { get; set; }

    public string FoodItemName { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsVegetarian { get; set; }

    public int RestaurantId { get; set; }
}