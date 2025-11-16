using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Slug)
            .HasMaxLength(75);

        builder.Property(t => t.Color)
            .HasMaxLength(7);

        builder.HasIndex(t => t.Slug);
    }
}