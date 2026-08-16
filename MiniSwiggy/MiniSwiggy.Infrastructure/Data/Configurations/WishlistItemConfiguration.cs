using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Wishlist)
               .WithMany(x => x.WishlistItems)
               .HasForeignKey(x => x.WishlistId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FoodItem)
               .WithMany(x => x.WishlistItems)
               .HasForeignKey(x => x.FoodItemId)
               .OnDelete(DeleteBehavior.Restrict);

        // One food item should appear only once in a user's wishlist
        builder.HasIndex(x => new { x.WishlistId, x.FoodItemId })
               .IsUnique();
    }
}