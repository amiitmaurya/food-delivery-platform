using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Review;

public class ReviewResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? UserImageUrl { get; set; }

    public int FoodItemId { get; set; }

    public string? FoodName { get; set; }

    public string? FoodImageUrl { get; set; }

    public string? RestaurantName { get; set; }

    public string? RestaurantImageUrl { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedOn { get; set; }
}
