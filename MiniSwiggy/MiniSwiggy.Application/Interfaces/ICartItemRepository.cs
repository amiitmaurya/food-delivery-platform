using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task<CartItem?> GetByCartAndFoodItemAsync(int cartId, int foodItemId);

    Task<IEnumerable<CartItem>> GetByCartIdAsync(int cartId);


} 