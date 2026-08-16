using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(300);

        builder.Property(x => x.DiscountType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DiscountValue)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.MinimumOrderAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.MaximumDiscount)
            .HasColumnType("decimal(18,2)");
    }
}
