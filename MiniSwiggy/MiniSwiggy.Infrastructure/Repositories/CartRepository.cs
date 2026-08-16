using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;

namespace MiniSwiggy.Infrastructure.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _context.Carts
    .Include(c => c.CartItems)
        .ThenInclude(ci => ci.FoodItem)
            .ThenInclude(fi => fi.Category)
    .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}