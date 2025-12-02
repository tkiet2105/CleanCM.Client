using CleanCCM.Domain.Common;
using CleanCCM.Domain.Exceptions;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Category phân loại sản phẩm
public class Category : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Slug { get; private set; }
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();

    private Category() : base() { }

    public static Category Create(string name,string icon, string description, int displayOrder = 0)
    {
        var category = new Category
        {
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            Slug = name.ToSlug(),
            IsActive = true,
            Icon = icon
        };

        return category;
    }

    public void UpdateInfo(string name, string description)
    {
        Name = name;
        Description = description;
        Slug = name.ToSlug();
    }

    public void SetIcon(string icon)
    {
        Icon = icon;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
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