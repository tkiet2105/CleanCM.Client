using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;




/// <summary>
/// ProductComment configuration
/// </summary>
public class ProductCommentConfiguration : IEntityTypeConfiguration<ProductComment>
{
    public void Configure(EntityTypeBuilder<ProductComment> builder)
    {
        builder.ToTable("ProductComments");

        builder.HasKey(pc => pc.Id);

        // Properties
        builder.Property(pc => pc.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(pc => pc.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pc => pc.ReportReason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(pc => pc.ProductId);

        builder.HasIndex(pc => pc.UserId);

        builder.HasIndex(pc => pc.ParentCommentId);

        builder.HasIndex(pc => pc.Status);

        builder.HasIndex(pc => new { pc.ProductId, pc.Status });

        // Relationships
        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductComments)
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing (Reply)
        builder.HasOne(pc => pc.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(pc => pc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

