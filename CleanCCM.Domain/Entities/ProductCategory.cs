using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Bảng trung gian cho quan hệ many-to-many giữa Product và Category
public class ProductCategory
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }

    private ProductCategory() { }

    public static ProductCategory Create(Guid productId, Guid categoryId, string? assignedBy = null)
    {
        return new ProductCategory
        {
            ProductId = productId,
            CategoryId = categoryId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };
    }
}