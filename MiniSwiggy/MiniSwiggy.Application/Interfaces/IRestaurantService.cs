using Microsoft.AspNetCore.Http;
using MiniSwiggy.Application.DTOs.Restaurant;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IRestaurantService
{
    Task<IEnumerable<RestaurantResponse>> GetAllAsync();

    Task<RestaurantResponse?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreateRestaurantRequest request);

    Task<bool> UpdateAsync(UpdateRestaurantRequest request);
    Task<string> UploadImageAsync(int restaurantId, IFormFile file);

    Task<bool> DeleteAsync(int id);
}
 