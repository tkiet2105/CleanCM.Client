using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;



/// <summary>
/// ProductTag junction table configuration
/// </summary>
public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("ProductTags");

        builder.HasKey(pt => pt.Id);

        // Composite unique index
        builder.HasIndex(pt => new { pt.ProductId, pt.TagId })
            .IsUnique();

        // Relationships
        builder.HasOne(pt => pt.Product)
            .WithMany(p => p.ProductTags)
            .HasForeignKey(pt => pt.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Tag)
            .WithMany(t => t.ProductTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pt => pt.ProductId);
        builder.HasIndex(pt => pt.TagId);
    }
}
