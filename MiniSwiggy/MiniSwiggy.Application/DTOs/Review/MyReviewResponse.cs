using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Review;

public class MyReviewResponse
{
    public int Id { get; set; }

    public string FoodName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedOn { get; set; }
}
