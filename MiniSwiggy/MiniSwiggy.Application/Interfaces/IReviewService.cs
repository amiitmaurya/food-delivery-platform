using MiniSwiggy.Application.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.Interfaces;

public interface IReviewService
{
    Task<bool> AddReviewAsync(int userId, AddReviewRequest request);

    Task<bool> UpdateReviewAsync(int reviewId, UpdateReviewRequest request);

    Task<bool> DeleteReviewAsync(int reviewId);

    Task<FoodReviewResponse?> GetFoodReviewsAsync(int foodItemId);

    Task<IEnumerable<MyReviewResponse>> GetMyReviewsAsync(int userId);
    Task<RestaurantReviewResponse?> GetRestaurantReviewsAsync(int restaurantId);
    Task<IEnumerable<ReviewResponse>> GetAllReviewsAsync();
}
