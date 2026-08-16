using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Wishlist : BaseEntity
{
    public int UserId { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
