using MiniSwiggy.Application.DTOs.Cart;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;

namespace MiniSwiggy.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    

    public async Task<CartResponse?> GetMyCartAsync(int userId)
    {
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);

        if (cart == null)
            return null;

        return new CartResponse
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            TotalAmount = cart.TotalAmount,

            Items = cart.CartItems.Select(x => new CartItemResponse
            {
                Id = x.Id,
                FoodItemId = x.FoodItemId,
                FoodItemName = x.FoodItem.Name,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice,
                ImageUrl = x.FoodItem.ImageUrl,
                RestaurantId = x.FoodItem.RestaurantId,
                IsVegetarian = x.FoodItem.IsVegetarian
            }).ToList()
        };
    }

    
    public async Task<bool> AddToCartAsync(int userId, AddToCartRequest request)
    {
        if (request.Quantity <= 0)
            return false;

        // Check Food Item Exists
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(request.FoodItemId);

        if (foodItem == null)
            return false;

        // Check Food Item Availability
        if (!foodItem.IsAvailable)
            return false;

        // Get User Cart
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);

        // Create Cart If Not Exists
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                TotalAmount = 0
            };

            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        // Check Item Already Exists In Cart
        var cartItem = await _unitOfWork.CartItems.GetByCartAndFoodItemAsync(cart.Id, foodItem.Id);

        if (cartItem != null)
        {
            // Increase Quantity
            cartItem.Quantity += request.Quantity;
            cartItem.UnitPrice = foodItem.OfferPrice ?? foodItem.Price;
            cartItem.TotalPrice = cartItem.Quantity * cartItem.UnitPrice;

            _unitOfWork.CartItems.Update(cartItem);
        }
        else
        {
            // Add New Cart Item
            cartItem = new CartItem
            {
                CartId = cart.Id,
                FoodItemId = foodItem.Id,
                Quantity = request.Quantity,
                UnitPrice = foodItem.OfferPrice ?? foodItem.Price,
                TotalPrice = request.Quantity * (foodItem.OfferPrice ?? foodItem.Price)
            };

            await _unitOfWork.CartItems.AddAsync(cartItem);
        }

        // Save Cart Item Changes
        await _unitOfWork.SaveChangesAsync();

        // Recalculate Cart Total
        var items = await _unitOfWork.CartItems.GetByCartIdAsync(cart.Id);

        cart.TotalAmount = items.Sum(x => x.TotalPrice);

        _unitOfWork.Carts.Update(cart);

        // Save Cart Total
        await _unitOfWork.SaveChangesAsync();

        return true;
    }



    public async Task<bool> ClearCartAsync(int userId)
    {
        // Get User Cart
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId);

        if (cart == null)
            return false;

        // Get All Cart Items
        var items = await _unitOfWork.CartItems.GetByCartIdAsync(cart.Id);

        // Remove All Items
        foreach (var item in items)
        {
            _unitOfWork.CartItems.Delete(item);
        }

        // Reset Cart Total
        cart.TotalAmount = 0;

        _unitOfWork.Carts.Update(cart);

        // Save Changes
        await _unitOfWork.SaveChangesAsync();

        return true;
    }   

    public async Task<bool> RemoveCartItemAsync(int cartItemId)
    {
        // Get Cart Item
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);

        if (cartItem == null)
            return false;

        // Get Cart
        var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);

        if (cart == null)
            return false;

        // Remove Cart Item
        _unitOfWork.CartItems.Delete(cartItem);

        // Save Changes
        await _unitOfWork.SaveChangesAsync();

        // Recalculate Cart Total
        var items = await _unitOfWork.CartItems.GetByCartIdAsync(cart.Id);

        cart.TotalAmount = items.Sum(x => x.TotalPrice);

        _unitOfWork.Carts.Update(cart);

        // Save Cart Total
        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    public async Task<bool> UpdateCartItemAsync(int cartItemId, UpdateCartItemRequest request)
    {
        // Validate Quantity
        if (request.Quantity <= 0)
            return false;

        // Get Cart Item
        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);

        if (cartItem == null)
            return false;

        // Update Quantity
        cartItem.Quantity = request.Quantity;
        cartItem.TotalPrice = cartItem.Quantity * cartItem.UnitPrice;

        _unitOfWork.CartItems.Update(cartItem);

        // Save Cart Item
        await _unitOfWork.SaveChangesAsync();

        // Get Cart
        var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);

        if (cart == null)
            return false;

        // Recalculate Cart Total
        var items = await _unitOfWork.CartItems.GetByCartIdAsync(cart.Id);

        cart.TotalAmount = items.Sum(x => x.TotalPrice);

        _unitOfWork.Carts.Update(cart);

        // Save Cart Total
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _unitOfWork.Carts.GetByUserIdAsync(userId);
    }

}