using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.Property(x => x.Comment)
               .HasMaxLength(500);

        builder.Property(x => x.Rating)
               .IsRequired();

        builder.HasOne(x => x.User)
               .WithMany(x => x.Reviews)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FoodItem)
               .WithMany(x => x.Reviews)
               .HasForeignKey(x => x.FoodItemId)
               .OnDelete(DeleteBehavior.Cascade);

        // One review per user per food item
        builder.HasIndex(x => new
        {
            x.UserId,
            x.FoodItemId
        }).IsUnique();
    }
}
