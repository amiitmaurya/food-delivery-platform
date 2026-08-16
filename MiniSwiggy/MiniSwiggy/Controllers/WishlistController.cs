using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniSwiggy.Application.DTOs.Wishlist;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using System.Security.Claims;

namespace MiniSwiggy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly IUnitOfWork _unitOfWork;

    public WishlistController(IWishlistService wishlistService, IUnitOfWork unitOfWork)
    {
        _wishlistService = wishlistService;
        _unitOfWork = unitOfWork;
    }

    private int GetUserId()
    {
        var claim = User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }

    // Get My Wishlist (returns list of wishlisted restaurants for UI)
    [HttpGet]
    public async Task<IActionResult> GetMyWishlist()
    {
        try
        {
            int userId = GetUserId();
            var wishlist = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);

            if (wishlist == null || !wishlist.WishlistItems.Any())
                return Ok(new List<object>());

            var allRestaurants = await _unitOfWork.Restaurants.GetAllAsync();
            var wishlistedResIds = wishlist.WishlistItems
                .Select(x => x.FoodItem?.RestaurantId ?? (allRestaurants.Any(r => r.Id == x.FoodItemId) ? x.FoodItemId : 0))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var favRestaurants = allRestaurants
                .Where(r => wishlistedResIds.Contains(r.Id))
                .ToList();

            var result = favRestaurants.Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                CuisineType = "North Indian, Fast Food",
                r.Address,
                r.City,
                r.State,
                r.Pincode,
                PhoneNumber = r.MobileNumber,
                r.ImageUrl,
                r.Rating,
                DeliveryTimeInMins = r.DeliveryTime,
                CostForTwo = r.AverageCostForTwo,
                r.IsActive,
                r.IsOpen
            }).ToList();

            return Ok(result);
        }
        catch
        {
            return Ok(new List<object>());
        }
    }

    // Add Item
    [HttpPost("add")]
    public async Task<IActionResult> AddToWishlist(AddToWishlistRequest request)
    {
        var result = await _wishlistService.AddToWishlistAsync(GetUserId(), request);

        if (!result)
            return BadRequest(new { message = "Item already exists or food item not found." });

        return Ok(new { message = "Item added to wishlist." });
    }

    // Toggle Item (adds/removes restaurant or food item to wishlist DB)
    [HttpPost("toggle/{id}")]
    public async Task<IActionResult> ToggleWishlist(int id)
    {
        int userId = GetUserId();

        // Get or Create User Wishlist
        var wishlist = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);
        if (wishlist == null)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                var firstUser = (await _unitOfWork.Users.GetAllAsync()).FirstOrDefault();
                userId = firstUser?.Id ?? 1;
            }

            wishlist = new Wishlist { UserId = userId };
            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.SaveChangesAsync();
        }

        // Check if ID is a Restaurant
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);
        if (restaurant != null)
        {
            // Check if wishlist already has any item for this restaurant
            var existingItem = wishlist.WishlistItems.FirstOrDefault(x => x.FoodItem?.RestaurantId == id || x.FoodItemId == id);
            if (existingItem != null)
            {
                _unitOfWork.WishlistItems.Delete(existingItem);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Removed from wishlist", isWishlisted = false });
            }

            // Find or create a FoodItem for this restaurant so FK is valid
            var foods = await _unitOfWork.FoodItems.GetAllAsync();
            var resFood = foods.FirstOrDefault(f => f.RestaurantId == id);
            if (resFood == null)
            {
                var cat = (await _unitOfWork.Categories.GetAllAsync()).FirstOrDefault();
                resFood = new FoodItem
                {
                    Name = restaurant.Name + " Signature Dish",
                    Description = "Chef's special delight",
                    Price = 199,
                    RestaurantId = restaurant.Id,
                    CategoryId = cat?.Id ?? 1,
                    IsAvailable = true,
                    IsVegetarian = true
                };
                await _unitOfWork.FoodItems.AddAsync(resFood);
                await _unitOfWork.SaveChangesAsync();
            }

            var newItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                FoodItemId = resFood.Id
            };
            await _unitOfWork.WishlistItems.AddAsync(newItem);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Added to wishlist", isWishlisted = true });
        }
        else
        {
            // It's a food item
            var existingItem = wishlist.WishlistItems.FirstOrDefault(x => x.FoodItemId == id || x.Id == id);
            if (existingItem != null)
            {
                _unitOfWork.WishlistItems.Delete(existingItem);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Removed from wishlist", isWishlisted = false });
            }

            var food = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (food != null)
            {
                var newItem = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    FoodItemId = food.Id
                };
                await _unitOfWork.WishlistItems.AddAsync(newItem);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { message = "Added to wishlist", isWishlisted = true });
            }

            return BadRequest(new { message = "Restaurant or food item not found." });
        }
    }

    // Remove Item
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveWishlistItem(int id)
    {
        int userId = GetUserId();
        var wishlist = await _unitOfWork.Wishlists.GetByUserIdAsync(userId);
        if (wishlist != null)
        {
            var existing = wishlist.WishlistItems.FirstOrDefault(x => x.Id == id || x.FoodItemId == id || x.FoodItem?.RestaurantId == id);
            if (existing != null)
            {
                _unitOfWork.WishlistItems.Delete(existing);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return Ok(new { message = "Item removed successfully." });
    }

    // Clear Wishlist
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearWishlist()
    {
        var result = await _wishlistService.ClearWishlistAsync(GetUserId());

        if (!result)
            return BadRequest(new { message = "Wishlist is already empty." });

        return Ok(new { message = "Wishlist cleared successfully." });
    }
}