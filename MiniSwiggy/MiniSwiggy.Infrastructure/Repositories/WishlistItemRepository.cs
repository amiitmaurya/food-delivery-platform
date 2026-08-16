using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class WishlistItemRepository : Repository<WishlistItem>, IWishlistItemRepository
{
    private readonly ApplicationDbContext _context;

    public WishlistItemRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<WishlistItem?> GetByWishlistAndFoodItemAsync(int wishlistId, int foodItemId)
    {
        return await _context.WishlistItems
            .FirstOrDefaultAsync(x =>
                x.WishlistId == wishlistId &&
                x.FoodItemId == foodItemId &&
                !x.IsDeleted);
    }

    public async Task<IEnumerable<WishlistItem>> GetByWishlistIdAsync(int wishlistId)
    {
        return await _context.WishlistItems
            .Include(x => x.FoodItem)
            .Where(x =>
                x.WishlistId == wishlistId &&
                !x.IsDeleted)
            .ToListAsync();
    }
}
