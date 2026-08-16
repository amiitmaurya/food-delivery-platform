using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Repositories;

public class WishlistRepository : Repository<Wishlist>, IWishlistRepository
{
    private readonly ApplicationDbContext _context;

    public WishlistRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Wishlist?> GetByUserIdAsync(int userId)
    {
        return await _context.Wishlists
            .Include(x => x.WishlistItems)
                .ThenInclude(x => x.FoodItem)
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);
    }
}
