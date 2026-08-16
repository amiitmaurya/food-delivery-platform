using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetFoodReviewsAsync(int foodItemId);

    Task<IEnumerable<Review>> GetUserReviewsAsync(int userId);

    Task<Review?> GetUserReviewAsync(int userId, int foodItemId);
    Task<IEnumerable<Review>> GetRestaurantReviewsAsync(int restaurantId);
}
