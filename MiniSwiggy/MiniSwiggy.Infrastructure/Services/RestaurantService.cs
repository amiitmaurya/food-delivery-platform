using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniSwiggy.Application.DTOs.Restaurant;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Domain.Enums;
using MiniSwiggy.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ILogger<RestaurantService> _logger;

    public RestaurantService(IUnitOfWork unitOfWork, IFileService fileService, ILogger<RestaurantService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<IEnumerable<RestaurantResponse>> GetAllAsync()
    {
        var restaurants = await _unitOfWork.Restaurants.GetAllActiveAsync();
        var allFoodItems = await _unitOfWork.FoodItems.GetAllAsync();
        var allOrders = await _unitOfWork.Orders.GetAllAsync();

        var resIdsWithFood = new HashSet<int>(allFoodItems.Where(f => !f.IsDeleted).Select(f => f.RestaurantId));
        var resIdsWithOrders = new HashSet<int>(allOrders.Select(o => o.RestaurantId));

        return restaurants.Select(x => new RestaurantResponse
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            CuisineType = x.CuisineType,
            Address = x.Address,
            City = x.City,
            Rating = x.Rating,
            DeliveryTime = x.DeliveryTime,
            DeliveryCharge = x.DeliveryCharge,
            MinimumOrderAmount = x.MinimumOrderAmount,
            IsOpen = x.IsOpen,
            ImageUrl = x.ImageUrl,
            Logo = x.Logo,
            BannerImage = x.BannerImage,
            OwnerName = x.OwnerName,
            MobileNumber = x.MobileNumber,
            Email = x.Email,
            State = x.State,
            Pincode = x.Pincode,
            AverageCostForTwo = x.AverageCostForTwo,
            OpeningTime = x.OpeningTime,
            ClosingTime = x.ClosingTime,
            HasFoodItems = resIdsWithFood.Contains(x.Id),
            HasOrders = resIdsWithOrders.Contains(x.Id)
        });
    }

    public async Task<RestaurantResponse?> GetByIdAsync(int id)
    {
        var restaurant = await _unitOfWork.Restaurants.GetRestaurantByIdAsync(id);

        if (restaurant == null)
            return null;

        return new RestaurantResponse
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Description = restaurant.Description,
            CuisineType = restaurant.CuisineType,
            Address = restaurant.Address,
            City = restaurant.City,
            Rating = restaurant.Rating,
            DeliveryTime = restaurant.DeliveryTime,
            DeliveryCharge = restaurant.DeliveryCharge,
            MinimumOrderAmount = restaurant.MinimumOrderAmount,
            IsOpen = restaurant.IsOpen,
            ImageUrl = restaurant.ImageUrl,
            Logo = restaurant.Logo,
            BannerImage = restaurant.BannerImage,
            OwnerName = restaurant.OwnerName,
            MobileNumber = restaurant.MobileNumber,
            Email = restaurant.Email,
            State = restaurant.State,
            Pincode = restaurant.Pincode,
            AverageCostForTwo = restaurant.AverageCostForTwo,
            OpeningTime = restaurant.OpeningTime,
            ClosingTime = restaurant.ClosingTime,
            IsVerified = restaurant.IsVerified,
        };
    }

    public async Task<bool> UpdateAsync(UpdateRestaurantRequest request)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(request.Id);

        if (restaurant == null)
            return false;

        restaurant.Name = request.Name;
        restaurant.Description = request.Description;
        restaurant.CuisineType = request.CuisineType;
        restaurant.Address = request.Address;
        restaurant.City = request.City;
        restaurant.DeliveryTime = request.DeliveryTime;
        restaurant.DeliveryCharge = request.DeliveryCharge;
        restaurant.MinimumOrderAmount = request.MinimumOrderAmount;
        restaurant.IsOpen = request.IsOpen;
        restaurant.Logo = request.Logo;
        restaurant.BannerImage = request.BannerImage;
        restaurant.OwnerName = request.OwnerName;
        restaurant.MobileNumber = request.MobileNumber;
        restaurant.Email = request.Email;
        restaurant.State = request.State;
        restaurant.Pincode = request.Pincode;
        restaurant.AverageCostForTwo = request.AverageCostForTwo;
        restaurant.OpeningTime = request.OpeningTime;
        restaurant.ClosingTime = request.ClosingTime;
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            restaurant.ImageUrl = request.ImageUrl;
        }

        _unitOfWork.Restaurants.Update(restaurant);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<int> CreateAsync(CreateRestaurantRequest request)
    {
        var restaurant = new Restaurant
        {
            Name = request.Name,
            Description = request.Description,
            CuisineType = request.CuisineType,
            OwnerName = request.OwnerName,
            MobileNumber = request.MobileNumber,
            Email = request.Email,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Pincode = request.Pincode,
            DeliveryTime = request.DeliveryTime,
            DeliveryCharge = request.DeliveryCharge,
            MinimumOrderAmount = request.MinimumOrderAmount,
            AverageCostForTwo = request.AverageCostForTwo,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            ImageUrl = request.ImageUrl,
            Rating = request.Rating,
            IsOpen = request.IsOpen,
            IsActive = request.IsActive
        };

        await _unitOfWork.Restaurants.AddAsync(restaurant);

        await _unitOfWork.SaveChangesAsync();

        return restaurant.Id;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(id);

        if (restaurant == null)
            return false;

        var allFoodItems = await _unitOfWork.FoodItems.GetAllAsync();
        if (allFoodItems.Any(f => f.RestaurantId == id && !f.IsDeleted))
        {
            throw new BadRequestException("Cannot delete restaurant because it has active food items. Delete food items first.");
        }

        var allOrders = await _unitOfWork.Orders.GetAllAsync();
        if (allOrders.Any(o => o.RestaurantId == id))
        {
            throw new BadRequestException("Cannot delete restaurant because it has associated customer orders.");
        }

        try
        {
            _unitOfWork.Restaurants.Delete(restaurant);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new BadRequestException("Cannot delete restaurant because it is referenced in existing records.");
        }
    }

    public async Task<string> UploadImageAsync(int restaurantId, IFormFile file)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);

        if (restaurant is null)
            throw new NotFoundException("Restaurant not found.");

        // Purani image ka path save kar lo
        var oldImageUrl = restaurant.ImageUrl;

        // Pehle nayi image upload karo
        var newImageUrl = await _fileService.UploadImageAsync(
            file,
            UploadFolder.Restaurants);

        try
        {
            // Database update
            restaurant.ImageUrl = newImageUrl;

            _unitOfWork.Restaurants.Update(restaurant);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
    "Restaurant image updated successfully. RestaurantId: {RestaurantId}",
    restaurantId);

            // Database save hone ke baad hi purani image delete karo
            if (!string.IsNullOrWhiteSpace(oldImageUrl))
            {
                try
                {
                    await _fileService.DeleteImageAsync(oldImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old restaurant image.");
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
                _logger.LogWarning(ex, "Failed to rollback uploaded restaurant image.");
            }

            throw;
        }
    }

}
