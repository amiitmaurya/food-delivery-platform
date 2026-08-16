namespace MiniSwiggy.Application.DTOs.FoodItem;

public class UpdateFoodItemRequest
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? OfferPrice { get; set; }

    public bool IsVeg { get; set; }
    public bool IsVegetarian { get => IsVeg; set => IsVeg = value; }

    public string? Image { get; set; }

    public bool IsAvailable { get; set; }
}

      