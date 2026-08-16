using MiniSwiggy.Application.DTOs.Cart;
using MiniSwiggy.Domain.Entities;


namespace MiniSwiggy.Application.Interfaces;

public interface ICartService
{
    Task<CartResponse?> GetMyCartAsync(int userId);
    Task<Cart?> GetByUserIdAsync(int userId);

    Task<bool> AddToCartAsync(int userId, AddToCartRequest request);

    Task<bool> UpdateCartItemAsync(int cartItemId, UpdateCartItemRequest request);

    Task<bool> RemoveCartItemAsync(int cartItemId);

    Task<bool> ClearCartAsync(int userId);
} 