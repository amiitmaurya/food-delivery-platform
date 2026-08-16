using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniSwiggy.Application.DTOs.Common;
using MiniSwiggy.Application.DTOs.FoodItem;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Enums;
using MiniSwiggy.Shared.Exceptions;


namespace MiniSwiggy.Infrastructure.Services;

public class FoodItemService : IFoodItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ILogger<FoodItemService> _logger;

    public FoodItemService(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    ILogger<FoodItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<IEnumerable<FoodItemResponse>> GetByCategoryAsync(int categoryId)
    {
        var foodItems = await _unitOfWork.FoodItems.GetByCategoryAsync(categoryId);

        return foodItems.Select(x => new FoodItemResponse
        {
            Id = x.Id,
            CategoryId = x.CategoryId,
            RestaurantId = x.RestaurantId,
            Name = x.Name,
            Description = x.Description,
            Price = x.Price,
            OfferPrice = x.OfferPrice,
            IsVeg = x.IsVeg || x.IsVegetarian,
            Rating = x.Rating,
            Image = x.Image,
            IsAvailable = x.IsAvailable
        });
    }

    public async Task<FoodItemResponse?> GetByIdAsync(int id)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);

        if (foodItem == null)
            return null;

        return new FoodItemResponse
        {
            Id = foodItem.Id,
            CategoryId = foodItem.CategoryId,
            RestaurantId = foodItem.RestaurantId,
            Name = foodItem.Name,
            ImageUrl = foodItem.ImageUrl,
            Description = foodItem.Description,
            Price = foodItem.Price,
            OfferPrice = foodItem.OfferPrice,
            IsVeg = foodItem.IsVeg || foodItem.IsVegetarian,
            Rating = foodItem.Rating,
            Image = foodItem.Image,
            IsAvailable = foodItem.IsAvailable
        };
    }

    public async Task<int> CreateAsync(CreateFoodItemRequest request)
    {
        // Resolve Category & Restaurant
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);

        int targetRestaurantId = request.RestaurantId;
        if (targetRestaurantId <= 0 && category != null)
        {
            targetRestaurantId = category.RestaurantId;
        }

        if (targetRestaurantId <= 0)
        {
            var restaurants = await _unitOfWork.Restaurants.GetAllAsync();
            var firstRes = restaurants.FirstOrDefault();
            if (firstRes != null)
            {
                targetRestaurantId = firstRes.Id;
            }
        }

        // If category is null but we have a valid restaurant, or default category exists
        int targetCategoryId = request.CategoryId;
        if (category == null)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var firstCat = categories.FirstOrDefault();
            if (firstCat != null)
            {
                targetCategoryId = firstCat.Id;
                if (targetRestaurantId <= 0) targetRestaurantId = firstCat.RestaurantId;
            }
        }

        var foodItem = new FoodItem
        {
            CategoryId = targetCategoryId > 0 ? targetCategoryId : 1,
            RestaurantId = targetRestaurantId > 0 ? targetRestaurantId : 1,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            OfferPrice = request.OfferPrice,
            IsVeg = request.IsVeg,
            IsVegetarian = request.IsVeg,
            Image = request.Image,
            IsAvailable = request.IsAvailable
        };

        await _unitOfWork.FoodItems.AddAsync(foodItem);

        await _unitOfWork.SaveChangesAsync();

        return foodItem.Id;
    }

    public async Task<bool> UpdateAsync(UpdateFoodItemRequest request)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(request.Id);

        if (foodItem == null)
            return false;

        foodItem.CategoryId = request.CategoryId > 0 ? request.CategoryId : foodItem.CategoryId;
        if (request.RestaurantId > 0)
        {
            foodItem.RestaurantId = request.RestaurantId;
        }
        foodItem.Name = request.Name;
        foodItem.Description = request.Description;
        foodItem.Price = request.Price;
        foodItem.OfferPrice = request.OfferPrice;
        foodItem.IsVeg = request.IsVeg;
        foodItem.IsVegetarian = request.IsVeg;
        if (!string.IsNullOrWhiteSpace(request.Image))
        {
            foodItem.Image = request.Image;
            foodItem.ImageUrl = request.Image;
        }
        foodItem.IsAvailable = request.IsAvailable;

        _unitOfWork.FoodItems.Update(foodItem);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);

        if (foodItem == null)
            return false;

        var orderItems = await _unitOfWork.OrderItems.GetAllAsync();
        if (orderItems.Any(oi => oi.FoodItemId == id))
        {
            throw new BadRequestException("Cannot delete this food item because it has been ordered by customers. You can mark it as unavailable instead.");
        }

        try
        {
            _unitOfWork.FoodItems.Delete(foodItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new BadRequestException("Cannot delete this food item because it is referenced in existing orders or cart items.");
        }
    }

    public async Task<List<FoodItemResponse>> GetAllAsync()
    {
        var foods = await _unitOfWork.FoodItems.GetAllAsync();
        var orderItems = await _unitOfWork.OrderItems.GetAllAsync();
        var orderedFoodIds = new HashSet<int>(orderItems.Select(oi => oi.FoodItemId));

        return foods
            .Where(x => !x.IsDeleted)
            .Select(x => new FoodItemResponse
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                RestaurantId = x.RestaurantId,
                Name = x.Name,
                ImageUrl = x.ImageUrl,
                Description = x.Description,
                Price = x.Price,
                OfferPrice = x.OfferPrice,
                IsVeg = x.IsVeg || x.IsVegetarian,
                Rating = x.Rating,
                Image = x.Image,
                IsAvailable = x.IsAvailable,
                HasOrders = orderedFoodIds.Contains(x.Id)
            }).ToList();
    }

    public async Task<PagedResponse<FoodItemResponse>> SearchFoodsAsync(FoodFilterRequest request)
    {
        var result = await _unitOfWork.FoodItems.SearchFoodsAsync(request);

        return new PagedResponse<FoodItemResponse>
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalRecords = result.TotalRecords,
            TotalPages = result.TotalPages,

            Items = result.Items.Select(x => new FoodItemResponse
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                RestaurantId = x.RestaurantId,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                OfferPrice = x.OfferPrice,
                IsVeg = x.IsVeg,
                Rating = x.Rating,
                Image = x.Image,
                IsAvailable = x.IsAvailable
            })
        };
    }

    public async Task<string> UploadImageAsync(int foodItemId, IFormFile file)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(foodItemId);

        if (foodItem is null)
            throw new NotFoundException("Food item not found.");

        var oldImageUrl = foodItem.ImageUrl;

        var newImageUrl = await _fileService.UploadImageAsync(
            file,
            UploadFolder.FoodItems);

        try
        {
            foodItem.ImageUrl = newImageUrl;
            foodItem.Image = newImageUrl;

            _unitOfWork.FoodItems.Update(foodItem);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Food item image uploaded successfully. FoodItemId: {FoodItemId}",
                foodItemId);

            if (!string.IsNullOrWhiteSpace(oldImageUrl))
            {
                try
                {
                    await _fileService.DeleteImageAsync(oldImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to delete old food item image.");
                }
            }

            return newImageUrl;
        }
        catch
        {
            try
            {
                await _fileService.DeleteImageAsync(newImageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to rollback uploaded food item image.");
            }

            throw;
        }
    }
}