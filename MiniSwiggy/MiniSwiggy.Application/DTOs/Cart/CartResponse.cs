using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Cart;

public class CartResponse
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public List<CartItemResponse> Items { get; set; } = [];
}
