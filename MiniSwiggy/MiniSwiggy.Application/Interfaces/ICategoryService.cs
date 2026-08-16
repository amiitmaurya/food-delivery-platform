using Microsoft.AspNetCore.Http;
using MiniSwiggy.Application.DTOs.Category;

namespace MiniSwiggy.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllByRestaurantAsync(int restaurantId);

    Task<CategoryResponse?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreateCategoryRequest request);


    Task<bool> UpdateAsync(UpdateCategoryRequest request);
    Task<string> UploadImageAsync(
    int categoryId,
    IFormFile file);

    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<CategoryResponse>> GetAllAsync();
}