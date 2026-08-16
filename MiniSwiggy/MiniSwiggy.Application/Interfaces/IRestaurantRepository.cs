using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IRestaurantRepository : IRepository<Restaurant>
{
    Task<IEnumerable<Restaurant>> GetAllActiveAsync();

    Task<Restaurant?> GetRestaurantByIdAsync(int id);

    Task<bool> ExistsAsync(string name);
} 