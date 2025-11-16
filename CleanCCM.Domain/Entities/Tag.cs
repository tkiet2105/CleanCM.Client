using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Tag đánh dấu và phân loại sản phẩm theo từ khóa
public class Tag : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Slug { get; private set; }
    public string? Color { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();

    private Tag() : base() { }

    public static Tag Create(string name, string? color = null)
    {
        var tag = new Tag
        {
            Name = name,
            Slug = GenerateSlug(name),
            Color = color ?? "#6B7280",
            IsActive = true
        };

        return tag;
    }

    public void UpdateInfo(string name, string? color)
    {
        Name = name;
        Slug = GenerateSlug(name);
        if (color != null)
            Color = color;
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
    public void Activate()
    {
        if (!IsActive)
            IsActive = true;
    }

    public void Deactivate()
    {
        if (IsActive)
            IsActive = false;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}