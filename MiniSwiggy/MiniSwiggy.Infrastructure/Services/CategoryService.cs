using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniSwiggy.Application.DTOs.Category;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Enums;
using MiniSwiggy.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork,
    IFileService fileService,
    ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllByRestaurantAsync(int restaurantId)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();

        return categories
            .Where(x => !x.IsDeleted && (restaurantId == 0 || x.RestaurantId == restaurantId))
            .Select(x => new CategoryResponse
            {
                Id = x.Id,
                RestaurantId = x.RestaurantId,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            });
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null)
            return null;

        return new CategoryResponse
        {
            Id = category.Id,
            RestaurantId = category.RestaurantId,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }

    public async Task<int> CreateAsync(CreateCategoryRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(request.RestaurantId);
        int targetRestaurantId = request.RestaurantId;

        if (restaurant == null)
        {
            var restaurants = await _unitOfWork.Restaurants.GetAllAsync();
            var firstRestaurant = restaurants.FirstOrDefault();
            if (firstRestaurant != null)
            {
                targetRestaurantId = firstRestaurant.Id;
            }
            else
            {
                var defaultRes = new Restaurant
                {
                    Name = "Dominaaz",
                    Description = "Delicious Food",
                    OwnerName = "Admin",
                    MobileNumber = "9999999999",
                    Email = "admin@miniswiggy.com",
                    Address = "12, Lucknow",
                    City = "Lucknow",
                    State = "Uttar Pradesh",
                    Pincode = "226001",
                    Rating = 4.5m,
                    DeliveryTime = 30,
                    DeliveryCharge = 30,
                    MinimumOrderAmount = 100,
                    AverageCostForTwo = 250,
                    IsOpen = true,
                    IsActive = true
                };
                await _unitOfWork.Restaurants.AddAsync(defaultRes);
                await _unitOfWork.SaveChangesAsync();
                targetRestaurantId = defaultRes.Id;
            }
        }

        var exists = await _unitOfWork.Categories.ExistsAsync(targetRestaurantId, request.Name);
        if (exists)
            return 0;

        var category = new Category
        {
            RestaurantId = targetRestaurantId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1,
            ImageUrl = request.ImageUrl,
            IsActive = true
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category.Id;
    }



    public async Task<bool> UpdateAsync(UpdateCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id);

        if (category == null)
            return false;

        category.Name = request.Name;
        category.Description = request.Description;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            category.ImageUrl = request.ImageUrl;
        }

        _unitOfWork.Categories.Update(category);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null)
            return false;

        var foodItems = await _unitOfWork.FoodItems.GetByCategoryAsync(id);
        if (foodItems.Any(f => !f.IsDeleted))
        {
            throw new BadRequestException("Cannot delete category because it contains active food items. Delete or reassign food items first.");
        }

        try
        {
            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new BadRequestException("Cannot delete category because it is referenced by existing items.");
        }
    }

    public async Task<string> UploadImageAsync(int categoryId, IFormFile file)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

        if (category is null)
            throw new NotFoundException("Category not found.");

        // Purani image ka path save kar lo
        var oldImageUrl = category.ImageUrl;

        // Pehle nayi image upload karo
        var newImageUrl = await _fileService.UploadImageAsync(
            file,
            UploadFolder.Categories);

        try
        {
            // Database update
            category.ImageUrl = newImageUrl;

            _unitOfWork.Categories.Update(category);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Category image uploaded successfully. CategoryId: {CategoryId}",
                categoryId);

            // Database save hone ke baad hi purani image delete karo
            if (!string.IsNullOrWhiteSpace(oldImageUrl))
            {
                try
                {
                    await _fileService.DeleteImageAsync(oldImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to delete old category image.");
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
                    "Failed to rollback uploaded category image.");
            }

            throw;
        }
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        var allFoodItems = await _unitOfWork.FoodItems.GetAllAsync();
        var categoryIdsWithFood = new HashSet<int>(allFoodItems.Where(f => !f.IsDeleted).Select(f => f.CategoryId));

        return categories.Select(x => new CategoryResponse
        {
            Id = x.Id,
            RestaurantId = x.RestaurantId,
            Name = x.Name,
            Description = x.Description,
            ImageUrl = x.ImageUrl,
            DisplayOrder = x.DisplayOrder,
            IsActive = x.IsActive,
            HasFoodItems = categoryIdsWithFood.Contains(x.Id)
        });
    }
}
