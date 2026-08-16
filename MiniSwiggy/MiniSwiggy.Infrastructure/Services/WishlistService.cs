using MiniSwiggy.Application.DTOs.Wishlist;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _unitOfWork;

    public WishlistService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WishlistResponse?> GetMyWishlistAsync(int userId)
    {
        var wishlist = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);

        if (wishlist == null)
            return null;

        return new WishlistResponse
        {
            WishlistId = wishlist.Id,
            UserId = wishlist.UserId,

            Items = wishlist.WishlistItems.Select(x => new WishlistItemResponse
            {
                Id = x.Id,
                FoodItemId = x.FoodItemId,
                FoodName = x.FoodItem.Name,
                Price = x.FoodItem.Price,
                OfferPrice = x.FoodItem.OfferPrice,
                Image = x.FoodItem.Image,
                IsVeg = x.FoodItem.IsVeg,
                Rating = x.FoodItem.Rating
            }).ToList()
        };
    }


    public async Task<bool> AddToWishlistAsync(int userId, AddToWishlistRequest request)
    {
        // Check Food Item Exists
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(request.FoodItemId);

        if (foodItem == null)
            return false;

        // Get User Wishlist
        var wishlist = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);

        // Create Wishlist If Not Exists
        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                UserId = userId
            };

            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
        }

        // Check Duplicate Item
        var existingItem = await _unitOfWork.WishlistItems
            .GetByWishlistAndFoodItemAsync(wishlist.Id, request.FoodItemId);

        if (existingItem != null)
            return false;

        // Add Wishlist Item
        var wishlistItem = new WishlistItem
        {
            WishlistId = wishlist.Id,
            FoodItemId = request.FoodItemId
        };

        await _unitOfWork.WishlistItems.AddAsync(wishlistItem);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveWishlistItemAsync(int wishlistItemId)
    {
        var item = await _unitOfWork.WishlistItems.GetByIdAsync(wishlistItemId);

        if (item == null)
            return false;

        _unitOfWork.WishlistItems.Delete(item);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ClearWishlistAsync(int userId)
    {
        var wishlist = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);

        if (wishlist == null)
            return false;

        var items = await _unitOfWork.WishlistItems
            .GetByWishlistIdAsync(wishlist.Id);

        foreach (var item in items)
        {
            _unitOfWork.WishlistItems.Delete(item);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
