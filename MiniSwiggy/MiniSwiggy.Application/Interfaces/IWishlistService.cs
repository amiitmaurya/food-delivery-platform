using MiniSwiggy.Application.DTOs.Wishlist;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IWishlistService
{
    Task<WishlistResponse?> GetMyWishlistAsync(int userId);

    Task<bool> AddToWishlistAsync(int userId, AddToWishlistRequest request);

    Task<bool> RemoveWishlistItemAsync(int wishlistItemId);

    Task<bool> ClearWishlistAsync(int userId);
}
