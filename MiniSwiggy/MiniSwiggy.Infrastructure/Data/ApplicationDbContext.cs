using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Domain.Entities;
using MiniSwiggy.Infrastructure.Data.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Restaurant> Restaurants { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<FoodItem> FoodItems { get; set; }

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Wishlist> Wishlists { get; set; }

    public DbSet<WishlistItem> WishlistItems { get; set; }

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Coupon> Coupons => Set<Coupon>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<MiniSwiggy.Domain.Entities.DeliveryPartner.DeliveryPartnerProfile> DeliveryPartnerProfiles => Set<MiniSwiggy.Domain.Entities.DeliveryPartner.DeliveryPartnerProfile>();

    public DbSet<MiniSwiggy.Domain.Entities.DeliveryPartner.DeliveryOrderTracker> DeliveryOrderTrackers => Set<MiniSwiggy.Domain.Entities.DeliveryPartner.DeliveryOrderTracker>();

    public DbSet<UserModulePermission> UserModulePermissions => Set<UserModulePermission>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());

        modelBuilder.ApplyConfiguration(new ReviewConfiguration());

        modelBuilder.ApplyConfiguration(new AddressConfiguration());

        modelBuilder.ApplyConfiguration(new CouponConfiguration());

        modelBuilder.ApplyConfiguration(new PaymentConfiguration());

        modelBuilder.Entity<FoodItem>()
    .HasOne(f => f.Restaurant)
    .WithMany(r => r.FoodItems)
    .HasForeignKey(f => f.RestaurantId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FoodItem>()
            .HasOne(f => f.Category)
            .WithMany(c => c.FoodItems)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
