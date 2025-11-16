using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;



/// <summary>
/// ProductReaction configuration
/// </summary>
public class ProductReactionConfiguration : IEntityTypeConfiguration<ProductReaction>
{
    public void Configure(EntityTypeBuilder<ProductReaction> builder)
    {
        builder.ToTable("ProductReactions");

        builder.HasKey(pr => pr.Id);

        // Properties
        builder.Property(pr => pr.ReactionType)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(pr => pr.Metadata)
            .HasColumnType("nvarchar(max)"); // JSON

        // Indexes cho performance
        builder.HasIndex(pr => pr.ProductId);

        builder.HasIndex(pr => pr.UserId);

        builder.HasIndex(pr => pr.ReactionType);

        builder.HasIndex(pr => pr.ReactedAt);

        builder.HasIndex(pr => pr.IsActive);

        // Composite indexes cho queries phổ biến
        builder.HasIndex(pr => new { pr.ProductId, pr.ReactionType, pr.IsActive });

        builder.HasIndex(pr => new { pr.UserId, pr.ProductId, pr.ReactionType });

        builder.HasIndex(pr => new { pr.UserId, pr.ReactionType, pr.IsActive });

        // Relationship
        builder.HasOne(pr => pr.Product)
            .WithMany(p => p.ProductReactions)
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

