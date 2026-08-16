using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalAmount)
               .HasPrecision(18, 2);

        // One User -> One Cart
        builder.HasOne(x => x.User)
               .WithOne(x => x.Cart)
               .HasForeignKey<Cart>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // One Cart -> Many CartItems
        builder.HasMany(x => x.CartItems)
               .WithOne(x => x.Cart)
               .HasForeignKey(x => x.CartId)
               .OnDelete(DeleteBehavior.Cascade);

        // One active cart per user
        builder.HasIndex(x => x.UserId)
               .IsUnique();
    }
}
