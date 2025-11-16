using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;

/// <summary>
/// Product entity configuration
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.SKU)
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.DiscountPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.AverageRating)
            .HasColumnType("decimal(3,2)");

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        // Indexes cho performance
        builder.HasIndex(p => p.SKU)
            .IsUnique()
            .HasFilter("[SKU] IS NOT NULL");

        builder.HasIndex(p => p.Name);

        builder.HasIndex(p => p.DisplayOrder);

        builder.HasIndex(p => p.IsActive);

        builder.HasIndex(p => p.IsFeatured);

        // Indexes cho sorting
        builder.HasIndex(p => p.ViewCount);

        builder.HasIndex(p => p.LikeCount);

        builder.HasIndex(p => p.AverageRating);

        // Composite index cho queries phổ biến
        builder.HasIndex(p => new { p.IsActive, p.DisplayOrder });

        builder.HasIndex(p => new { p.IsActive, p.IsFeatured, p.DisplayOrder });
    }
}
