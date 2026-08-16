using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Restaurant;

public class RestaurantResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? CuisineType { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public decimal Rating { get; set; }

    public int DeliveryTime { get; set; }

    public decimal DeliveryCharge { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    public decimal AverageCostForTwo { get; set; }

    public TimeSpan OpeningTime { get; set; }

    public TimeSpan ClosingTime { get; set; }
    public string? ImageUrl { get; set; }
    public string? Logo { get; set; }

    public string? BannerImage { get; set; }

    public bool IsOpen { get; set; }

    public bool IsActive { get; set; }

    public bool IsVerified { get; set; }

    public bool HasOrders { get; set; }

    public bool HasFoodItems { get; set; }

    
}
