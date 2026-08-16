using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Review;

public class RestaurantReviewResponse
{
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public List<ReviewResponse> Reviews { get; set; } = new();
}
