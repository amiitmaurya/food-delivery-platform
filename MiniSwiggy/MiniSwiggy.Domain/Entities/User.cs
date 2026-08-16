using MiniSwiggy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

    public DateTime? LastLogin { get; set; }

    

    // Foreign Key
    public int RoleId { get; set; }

    // Navigation Property
    public Role Role { get; set; } = null!;
    public Cart? Cart { get; set; }
    public Wishlist? Wishlist { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
