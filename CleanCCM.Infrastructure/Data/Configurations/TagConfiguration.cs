using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;


/// <summary>
/// Tag entity configuration
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Slug)
            .HasMaxLength(100);

        builder.Property(t => t.Icon)
            .HasMaxLength(50);

        builder.Property(t => t.Color)
            .HasMaxLength(20); // #RRGGBB format

        // Indexes
        builder.HasIndex(t => t.Slug)
            .IsUnique()
            .HasFilter("[Slug] IS NOT NULL");

        builder.HasIndex(t => t.Name);

        builder.HasIndex(t => t.IsActive);

        builder.HasIndex(t => new { t.IsActive, t.DisplayOrder });
    }
}
