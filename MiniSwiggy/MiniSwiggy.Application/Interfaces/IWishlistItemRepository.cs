using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IWishlistItemRepository : IRepository<WishlistItem>
{
    Task<WishlistItem?> GetByWishlistAndFoodItemAsync(int wishlistId, int foodItemId);

    Task<IEnumerable<WishlistItem>> GetByWishlistIdAsync(int wishlistId);
}
