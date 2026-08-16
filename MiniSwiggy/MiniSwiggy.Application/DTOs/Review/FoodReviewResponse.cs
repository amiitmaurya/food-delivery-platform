using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Review;

public class FoodReviewResponse
{
    public int FoodItemId { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public List<ReviewResponse> Reviews { get; set; } = new();
}
