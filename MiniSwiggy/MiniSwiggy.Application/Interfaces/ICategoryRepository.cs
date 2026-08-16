using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetAllByRestaurantAsync(int restaurantId);
    Task<Category?> GetWithRestaurantAsync(int id);

    Task<bool> ExistsAsync(int restaurantId, string name);
}
