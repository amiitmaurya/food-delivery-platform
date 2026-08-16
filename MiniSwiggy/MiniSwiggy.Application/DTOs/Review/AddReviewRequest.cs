using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Review;

public class AddReviewRequest
{
    public int FoodItemId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}
