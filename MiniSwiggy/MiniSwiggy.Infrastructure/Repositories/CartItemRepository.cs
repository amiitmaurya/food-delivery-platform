using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data;

namespace MiniSwiggy.Infrastructure.Repositories;

public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    private readonly ApplicationDbContext _context;

    public CartItemRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CartItem?> GetByCartAndFoodItemAsync(int cartId, int foodItemId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cartId &&
                x.FoodItemId == foodItemId);
    }

    public async Task<Cart?> GetCartByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<IEnumerable<CartItem>> GetByCartIdAsync(int cartId)
    {
        return await _context.CartItems
            .Include(x => x.FoodItem)
            .Where(x => x.CartId == cartId)
            .ToListAsync();
    }
} 