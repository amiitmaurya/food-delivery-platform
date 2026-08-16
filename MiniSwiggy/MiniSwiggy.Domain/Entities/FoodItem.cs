using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class FoodItem : BaseEntity
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? OfferPrice { get; set; }

    public bool IsVeg { get; set; }

    public double Rating { get; set; }

    public string? Image { get; set; }

    public int RestaurantId { get; set; }
    public bool IsVegetarian { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public bool IsAvailable { get; set; } = true;

    public Category Category { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
