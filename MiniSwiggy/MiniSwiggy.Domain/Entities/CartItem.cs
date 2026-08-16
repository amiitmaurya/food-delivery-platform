using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }

    public int FoodItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Cart Cart { get; set; } = null!;

    public FoodItem FoodItem { get; set; } = null!;
}
