using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Cart;

public class AddToCartRequest
{
    public int FoodItemId { get; set; }

    public int Quantity { get; set; } = 1;
}