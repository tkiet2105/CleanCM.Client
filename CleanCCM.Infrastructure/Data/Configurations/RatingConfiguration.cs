using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Ratings)
            .HasForeignKey(r => r.ProductId);

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.Review)
            .HasMaxLength(1000);

        builder.HasIndex(r => new { r.ProductId, r.UserId })
            .IsUnique();
    }
}