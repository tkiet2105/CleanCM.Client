using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Category phân loại sản phẩm
public class Category : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Slug { get; private set; }
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }

    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    private Category() : base() { }

    public static Category Create(string name, string description, int displayOrder = 0)
    {
        var category = new Category
        {
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            Slug = GenerateSlug(name)
        };

        return category;
    }

    public void UpdateInfo(string name, string description)
    {
        Name = name;
        Description = description;
        Slug = GenerateSlug(name);
    }

    public void SetIcon(string icon)
    {
        Icon = icon;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Replace("á", "a")
            .Replace("à", "a")
            .Replace("ả", "a")
            .Replace("ã", "a")
            .Replace("ạ", "a");
    }
}