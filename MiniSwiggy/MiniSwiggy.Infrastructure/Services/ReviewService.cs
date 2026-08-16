using MiniSwiggy.Application.DTOs.Review;
using MiniSwiggy.Application.Interfaces;
using MiniSwiggy.Domain.Entities;

namespace MiniSwiggy.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> AddReviewAsync(int userId, AddReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return false;

        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(request.FoodItemId);

        if (foodItem == null || foodItem.IsDeleted)
            return false;

        var existingReview = await _unitOfWork.Reviews
            .GetUserReviewAsync(userId, request.FoodItemId);

        if (existingReview != null)
        {
            existingReview.Rating = request.Rating;
            existingReview.Comment = request.Comment;
            existingReview.IsDeleted = false;
            existingReview.UpdatedOn = DateTime.UtcNow;

            _unitOfWork.Reviews.Update(existingReview);
            await _unitOfWork.SaveChangesAsync();

            await UpdateFoodRatingAsync(request.FoodItemId);
            await UpdateRestaurantRatingAsync(foodItem.RestaurantId);

            return true;
        }


        var review = new Review
        {
            UserId = userId,
            FoodItemId = request.FoodItemId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.AddAsync(review);

        await _unitOfWork.SaveChangesAsync();

        await UpdateFoodRatingAsync(request.FoodItemId);
        await UpdateRestaurantRatingAsync(foodItem.RestaurantId);

        return true;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

        if (review == null)
            return false;

        var foodItemId = review.FoodItemId;
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(foodItemId);

        _unitOfWork.Reviews.Delete(review);

        await _unitOfWork.SaveChangesAsync();

        await UpdateFoodRatingAsync(foodItemId);
        if (foodItem != null)
        {
            await UpdateRestaurantRatingAsync(foodItem.RestaurantId);
        }

        return true;
    }

    public async Task<FoodReviewResponse?> GetFoodReviewsAsync(int foodItemId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(foodItemId);

        if (foodItem == null || foodItem.IsDeleted)
            return null;

        var reviews = (await _unitOfWork.Reviews.GetFoodReviewsAsync(foodItemId)).ToList();

        return new FoodReviewResponse
        {
            FoodItemId = foodItem.Id,
            FoodName = foodItem.Name,
            AverageRating = foodItem.Rating,
            TotalReviews = reviews.Count,
            Reviews = reviews.Select(x => new ReviewResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.User.FullName,
                UserImageUrl = x.User.ImageUrl,
                FoodItemId = x.FoodItemId,
                FoodName = x.FoodItem?.Name,
                FoodImageUrl = !string.IsNullOrWhiteSpace(x.FoodItem?.ImageUrl) ? x.FoodItem?.ImageUrl : x.FoodItem?.Image,
                RestaurantName = x.FoodItem?.Restaurant?.Name,
                RestaurantImageUrl = x.FoodItem?.Restaurant?.ImageUrl,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedOn = x.CreatedOn
            }).ToList()
        };
    }

    public async Task<IEnumerable<MyReviewResponse>> GetMyReviewsAsync(int userId)
    {
        var reviews = await _unitOfWork.Reviews.GetUserReviewsAsync(userId);

        return reviews.Select(x => new MyReviewResponse
        {
            Id = x.Id,
            FoodName = x.FoodItem.Name,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedOn = x.CreatedOn
        });
    }

    public async Task<bool> UpdateReviewAsync(int reviewId, UpdateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return false;

        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

        if (review == null)
            return false;

        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(review.FoodItemId);

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.IsDeleted = false;
        review.UpdatedOn = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);

        await _unitOfWork.SaveChangesAsync();

        await UpdateFoodRatingAsync(review.FoodItemId);
        if (foodItem != null)
        {
            await UpdateRestaurantRatingAsync(foodItem.RestaurantId);
        }

        return true;
    }

    private async Task UpdateFoodRatingAsync(int foodItemId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(foodItemId);

        if (foodItem == null)
            return;

        var reviews = await _unitOfWork.Reviews.GetFoodReviewsAsync(foodItemId);

        if (!reviews.Any())
        {
            foodItem.Rating = 0;
        }
        else
        {
            foodItem.Rating = Math.Round(reviews.Average(x => x.Rating), 1);
        }

        _unitOfWork.FoodItems.Update(foodItem);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateRestaurantRatingAsync(int restaurantId)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);

        if (restaurant == null)
            return;

        var reviews = await _unitOfWork.Reviews.GetRestaurantReviewsAsync(restaurantId);

        if (!reviews.Any())
        {
            restaurant.Rating = 0;
        }
        else
        {
            restaurant.Rating = (decimal)Math.Round(reviews.Average(x => x.Rating), 1);
        }

        _unitOfWork.Restaurants.Update(restaurant);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<RestaurantReviewResponse?> GetRestaurantReviewsAsync(int restaurantId)
    {
        var restaurant = await _unitOfWork.Restaurants.GetByIdAsync(restaurantId);

        if (restaurant == null || restaurant.IsDeleted)
            return null;

        var reviews = await _unitOfWork.Reviews.GetRestaurantReviewsAsync(restaurantId);

        var reviewList = reviews.ToList();

        var avgRating = reviewList.Any()
            ? Math.Round(reviewList.Average(x => x.Rating), 1)
            : 0;

        if (restaurant.Rating != (decimal)avgRating)
        {
            restaurant.Rating = (decimal)avgRating;
            _unitOfWork.Restaurants.Update(restaurant);
            await _unitOfWork.SaveChangesAsync();
        }

        return new RestaurantReviewResponse
        {
            RestaurantId = restaurant.Id,
            RestaurantName = restaurant.Name,
            AverageRating = avgRating,

            TotalReviews = reviewList.Count,

            Reviews = reviewList.Select(x => new ReviewResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.User?.FullName ?? "Customer",
                UserImageUrl = x.User?.ImageUrl,
                FoodItemId = x.FoodItemId,
                FoodName = x.FoodItem?.Name,
                FoodImageUrl = !string.IsNullOrWhiteSpace(x.FoodItem?.ImageUrl) ? x.FoodItem?.ImageUrl : x.FoodItem?.Image,
                RestaurantName = x.FoodItem?.Restaurant?.Name,
                RestaurantImageUrl = x.FoodItem?.Restaurant?.ImageUrl,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedOn = x.CreatedOn
            }).ToList()
        };
    }

    public async Task<IEnumerable<ReviewResponse>> GetAllReviewsAsync()
    {
        var reviews = await _unitOfWork.Reviews.FindAsync(r => !r.IsDeleted);
        var list = reviews.ToList();

        // Also fetch with navigation items if available
        return list.Select(x => new ReviewResponse
        {
            Id = x.Id,
            UserId = x.UserId,
            UserName = x.User?.FullName ?? "Customer",
            UserImageUrl = x.User?.ImageUrl,
            FoodItemId = x.FoodItemId,
            FoodName = x.FoodItem?.Name ?? "Food Item",
            FoodImageUrl = !string.IsNullOrWhiteSpace(x.FoodItem?.ImageUrl) ? x.FoodItem?.ImageUrl : x.FoodItem?.Image,
            RestaurantName = x.FoodItem?.Restaurant?.Name ?? "Restaurant",
            RestaurantImageUrl = x.FoodItem?.Restaurant?.ImageUrl,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedOn = x.CreatedOn
        }).OrderByDescending(x => x.Id).ToList();
    }
}