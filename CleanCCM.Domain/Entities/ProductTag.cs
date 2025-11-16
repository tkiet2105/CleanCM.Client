using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Bảng trung gian cho quan hệ many-to-many giữa Product và Tag
public class ProductTag
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }

    private ProductTag() { }

    public static ProductTag Create(Guid productId, Guid tagId, string? assignedBy = null)
    {
        return new ProductTag
        {
            ProductId = productId,
            TagId = tagId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };
    }
}