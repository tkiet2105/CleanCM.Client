using CleanCCM.Domain.Common;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CleanCCM.Domain.Entities;

// Sử dụng: Product chứa thông tin sản phẩm với relationships many-to-many đến Category và Tag
public class Product : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Slug { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsPublished { get; private set; }

    // Many-to-many relationships
    public ICollection<ProductCategory> ProductCategories { get; private set; } = new List<ProductCategory>();
    public ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();

    // One-to-many relationships
    public ICollection<Reaction> Reactions { get; private set; } = new List<Reaction>();
    public ICollection<Rating> Ratings { get; private set; } = new List<Rating>();
    public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

    private Product() : base() { }

    // Factory method
    public static Product Create(string name, string description, decimal price, int stock)
    {
        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            Slug = GenerateSlug(name),
            IsPublished = false
        };

        return product;
    }

    public void UpdateInfo(string name, string description, decimal price, int stock)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        Slug = GenerateSlug(name);
    }

    public void SetImage(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void Publish()
    {
        IsPublished = true;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }

    public void AddStock(int quantity)
    {
        Stock += quantity;
    }

    public void ReduceStock(int quantity)
    {
        if (Stock < quantity)
            throw new InvalidOperationException("Insufficient stock");

        Stock -= quantity;
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