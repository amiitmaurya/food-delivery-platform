using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Wishlist;

public class WishlistItemResponse
{
    public int Id { get; set; }

    public int FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal? OfferPrice { get; set; }

    public string? Image { get; set; }

    public bool IsVeg { get; set; }

    public double Rating { get; set; }
}
