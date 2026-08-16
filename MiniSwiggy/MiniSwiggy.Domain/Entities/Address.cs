using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class Address : BaseEntity
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string HouseNo { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string? Landmark { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsDefault { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
