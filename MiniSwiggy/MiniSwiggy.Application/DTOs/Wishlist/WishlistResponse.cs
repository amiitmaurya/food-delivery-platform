using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Wishlist;

public class WishlistResponse
{
    public int WishlistId { get; set; }

    public int UserId { get; set; }

    public List<WishlistItemResponse> Items { get; set; } = new();
}
