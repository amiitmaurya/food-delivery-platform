using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;

namespace MiniSwiggy.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllByRestaurantAsync(int restaurantId)
    {
        return await _context.Categories
            .Where(x => x.RestaurantId == restaurantId && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Category?> GetWithRestaurantAsync(int id)
    {
        return await _context.Categories
            .Include(x => x.Restaurant)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<bool> ExistsAsync(int restaurantId, string name)
    {
        return await _context.Categories
            .AnyAsync(x =>
                x.RestaurantId == restaurantId &&
                x.Name == name &&
                !x.IsDeleted);
    }


}
