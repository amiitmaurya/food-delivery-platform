using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Review : BaseEntity
{
    public int UserId { get; set; }

    public int FoodItemId { get; set; }

    // Rating 1 to 5
    public int Rating { get; set; }

    public string? Comment { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    public FoodItem FoodItem { get; set; } = null!;
}
