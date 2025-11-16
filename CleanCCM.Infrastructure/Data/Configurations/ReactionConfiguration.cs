using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Infrastructure.Data.Configurations;

public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reactions)
            .HasForeignKey(r => r.ProductId);

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(r => new { r.ProductId, r.UserId })
            .IsUnique();
    }
}