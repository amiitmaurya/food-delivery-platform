using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Review>> GetFoodReviewsAsync(int foodItemId)
    {
        return await _context.Reviews
            .Include(x => x.User)
            .Where(x => x.FoodItemId == foodItemId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetUserReviewsAsync(int userId)
    {
        return await _context.Reviews
            .Include(x => x.FoodItem)
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
    }

    public async Task<Review?> GetUserReviewAsync(int userId, int foodItemId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.FoodItemId == foodItemId);
    }

    public async Task<IEnumerable<Review>> GetRestaurantReviewsAsync(int restaurantId)
    {
        return await _context.Reviews
            .Where(r =>
                !r.IsDeleted &&
                !r.FoodItem.IsDeleted &&
                r.FoodItem.RestaurantId == restaurantId)
            .Include(r => r.User)
            .Include(r => r.FoodItem)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();
    }
}
