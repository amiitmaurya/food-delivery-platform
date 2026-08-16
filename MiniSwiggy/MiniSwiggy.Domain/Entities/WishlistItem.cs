using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public int WishlistId { get; set; }

    public int FoodItemId { get; set; }

    // Navigation Properties
    public Wishlist Wishlist { get; set; } = null!;

    public FoodItem FoodItem { get; set; } = null!;
}
