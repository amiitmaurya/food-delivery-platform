using MiniSwiggy.Domain.Common;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }

    public int FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    // Navigation Properties

    public Order Order { get; set; } = null!;

    public FoodItem FoodItem { get; set; } = null!;
}
