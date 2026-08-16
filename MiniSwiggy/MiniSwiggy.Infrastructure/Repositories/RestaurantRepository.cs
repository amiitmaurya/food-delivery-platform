using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class RestaurantRepository
    : Repository<Restaurant>, IRestaurantRepository
{
    private readonly ApplicationDbContext _context;

    public RestaurantRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Restaurant>> GetAllActiveAsync()
    {
        return await _context.Restaurants
            .Where(x => x.IsActive)
            .ToListAsync();
    }

    public async Task<Restaurant?> GetRestaurantByIdAsync(int id)
    {
        return await _context.Restaurants
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.Restaurants
            .AnyAsync(x => x.Name == name);
    }
}
