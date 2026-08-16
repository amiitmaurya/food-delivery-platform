using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice)
               .HasPrecision(18, 2);

        builder.Property(x => x.TotalPrice)
               .HasPrecision(18, 2);

        builder.HasOne(x => x.FoodItem)
               .WithMany(x => x.CartItems)
               .HasForeignKey(x => x.FoodItemId)
               .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate FoodItems in the same cart
        builder.HasIndex(x => new { x.CartId, x.FoodItemId })
               .IsUnique();
    }
} 
