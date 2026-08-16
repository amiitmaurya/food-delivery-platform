
using MiniSwiggy.Application.DTOs.Common;
using MiniSwiggy.Domain.Entities;


namespace MiniSwiggy.Application.Interfaces;

public interface IFoodItemRepository : IRepository<FoodItem>
{
    Task<IEnumerable<FoodItem>> GetByCategoryAsync(int categoryId);

    Task<bool> ExistsAsync(int categoryId, string name);


    Task<PagedResponse<FoodItem>> SearchFoodsAsync(FoodFilterRequest request);
}
