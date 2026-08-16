using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Restaurant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? CuisineType { get; set; }

    // 🟢 ADD
    public string OwnerName { get; set; } = string.Empty;

    // 🟢 ADD
    public string MobileNumber { get; set; } = string.Empty;

    // 🟢 ADD
    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    // 🟢 ADD
    public string State { get; set; } = string.Empty;

    // 🟢 ADD
    public string Pincode { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal Rating { get; set; }

    public int DeliveryTime { get; set; }

    public decimal DeliveryCharge { get; set; }

    public decimal Discount { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    // 🟢 ADD
    public decimal AverageCostForTwo { get; set; }

    // 🟢 ADD
    public TimeSpan OpeningTime { get; set; }

    // 🟢 ADD
    public TimeSpan ClosingTime { get; set; }


    public bool IsOpen { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? Logo { get; set; }

    public string? BannerImage { get; set; }

    // 🟢 ADD
    public bool IsVerified { get; set; } = false;

   

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
