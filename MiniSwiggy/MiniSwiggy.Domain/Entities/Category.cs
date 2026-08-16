using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Category : BaseEntity
{
    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }


    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Property
    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
 