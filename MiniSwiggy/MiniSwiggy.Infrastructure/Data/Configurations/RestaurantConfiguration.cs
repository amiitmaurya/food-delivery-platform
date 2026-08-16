using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");

        builder.HasKey(x => x.Id);

        // Unique Indexes
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.MobileNumber)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.CuisineType)
            .HasMaxLength(200);

        builder.Property(x => x.OwnerName)
            .HasMaxLength(100);

        builder.Property(x => x.MobileNumber)
            .HasMaxLength(15);

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(100);

        builder.Property(x => x.Pincode)
            .HasMaxLength(10);

        builder.Property(x => x.Rating)
            .HasPrecision(2, 1);

        builder.Property(x => x.DeliveryCharge)
            .HasPrecision(10, 2);

        builder.Property(x => x.MinimumOrderAmount)
            .HasPrecision(10, 2);

        builder.Property(x => x.AverageCostForTwo)
            .HasPrecision(18, 2);
    }
}