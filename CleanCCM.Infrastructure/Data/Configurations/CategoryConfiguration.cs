using CleanCCM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanCCM.Infrastructure.Data.Configurations;


/// <summary>
/// Category entity configuration
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .HasMaxLength(200);

        builder.Property(c => c.Icon)
            .HasMaxLength(50);
        // Indexes
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("[Slug] IS NOT NULL");

        builder.HasIndex(c => c.Name);

        builder.HasIndex(c => c.ParentCategoryId);



        // Self-referencing relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Không cho xóa cascade
    }
}
