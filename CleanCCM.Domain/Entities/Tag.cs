using CleanCCM.Domain.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // B1: chuẩn hóa Unicode
        string normalized = name.Normalize(NormalizationForm.FormD);

        // B2: loại bỏ toàn bộ dấu (accent)
        var builder = new StringBuilder();
        foreach (char c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }
        string noAccent = builder.ToString().Normalize(NormalizationForm.FormC);

        // B3: chuyển về lowercase
        noAccent = noAccent.ToLower();

        // B4: thay ký tự đặc biệt thành dấu "-"
        noAccent = Regex.Replace(noAccent, @"[^a-z0-9\s-]", "");

        // B5: đổi khoảng trắng thành "-"
        noAccent = Regex.Replace(noAccent, @"\s+", "-").Trim('-');

        // B6: bỏ "-" dư
        noAccent = Regex.Replace(noAccent, "-{2,}", "-");

        return noAccent;
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