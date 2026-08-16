using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.DTOs.Common;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;


namespace MiniSwiggy.Infrastructure.Repositories;

public class FoodItemRepository : Repository<FoodItem>, IFoodItemRepository
{
    private readonly ApplicationDbContext _context;

    public FoodItemRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FoodItem>> GetByCategoryAsync(int categoryId)
    {
        return await _context.FoodItems
            .Where(x => x.CategoryId == categoryId &&
                        x.IsAvailable &&
                        !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int categoryId, string name)
    {
        return await _context.FoodItems
            .AnyAsync(x => x.CategoryId == categoryId &&
                           x.Name == name &&
                           !x.IsDeleted);
    }

    public async Task<PagedResponse<FoodItem>> SearchFoodsAsync(FoodFilterRequest request)
    {
        var query = _context.FoodItems
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted && x.IsAvailable)
            .AsQueryable();

        // Keyword
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(x =>
                x.Name.Contains(request.Keyword) ||
                (x.Description != null && x.Description.Contains(request.Keyword)));
        }

        // Restaurant
        if (request.RestaurantId.HasValue)
        {
            query = query.Where(x =>
                x.Category.RestaurantId == request.RestaurantId.Value);
        }

        // Category
        if (request.CategoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == request.CategoryId.Value);
        }

        // Veg / Non-Veg
        if (request.IsVeg.HasValue)
        {
            query = query.Where(x =>
                x.IsVeg == request.IsVeg.Value);
        }

        // Price
        if (request.MinPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price <= request.MaxPrice.Value);
        }

        // Rating
        if (request.Rating.HasValue)
        {
            query = query.Where(x =>
                x.Rating >= request.Rating.Value);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "price" => query.OrderBy(x => x.Price),

            "price_desc" => query.OrderByDescending(x => x.Price),

            "rating" => query.OrderByDescending(x => x.Rating),

            "newest" => query.OrderByDescending(x => x.CreatedOn),

            _ => query.OrderBy(x => x.Name)
        };

        var totalRecords = await query.CountAsync();

        var foods = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponse<FoodItem>
        {
            Items = foods,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        };
    }
}
