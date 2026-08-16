using Microsoft.AspNetCore.Http;
using MiniSwiggy.Application.DTOs.Common;
using MiniSwiggy.Application.DTOs.FoodItem;

namespace MiniSwiggy.Application.Interfaces;

public interface IFoodItemService
{
    Task<IEnumerable<FoodItemResponse>> GetByCategoryAsync(int categoryId);

    Task<FoodItemResponse?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreateFoodItemRequest request);

    Task<bool> UpdateAsync(UpdateFoodItemRequest request);

    Task<bool> DeleteAsync(int id);

    Task<List<FoodItemResponse>> GetAllAsync();
    Task<string> UploadImageAsync(
    int foodItemId,
    IFormFile file);

    Task<PagedResponse<FoodItemResponse>> SearchFoodsAsync(FoodFilterRequest request);

}  