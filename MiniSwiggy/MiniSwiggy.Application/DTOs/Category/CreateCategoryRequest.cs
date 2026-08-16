using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Category;

public class CreateCategoryRequest
{
    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
