using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniSwiggy.Domain.Entities;

namespace MiniSwiggy.Infrastructure.Data.Configurations;

public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("FoodItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Price)
            .HasPrecision(10, 2);

        builder.Property(x => x.OfferPrice)
            .HasPrecision(10, 2);

        builder.Property(x => x.Image)
            .HasMaxLength(500);

        builder.Property(x => x.Rating)
            .HasDefaultValue(0);

        builder.Property(x => x.IsAvailable)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.FoodItems)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}          