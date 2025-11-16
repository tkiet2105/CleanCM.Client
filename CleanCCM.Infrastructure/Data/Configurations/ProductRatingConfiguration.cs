using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;




/// <summary>
/// ProductRating configuration
/// </summary>
public class ProductRatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("ProductRatings");

        builder.HasKey(pr => pr.Id);

        // Properties
        builder.Property(pr => pr.RatingValue)
            .IsRequired();

        builder.Property(pr => pr.Review)
            .HasMaxLength(1000);

        // Unique constraint: 1 user, 1 product, 1 rating
        builder.HasIndex(pr => new { pr.UserId, pr.ProductId })
            .IsUnique();

        // Indexes
        builder.HasIndex(pr => pr.ProductId);

        builder.HasIndex(pr => pr.UserId);

        builder.HasIndex(pr => pr.RatingValue);

        builder.HasIndex(pr => new { pr.ProductId, pr.RatingValue });

        // Relationship
        builder.HasOne(pr => pr.Product)
            .WithMany(p => p.ProductRatings)
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}