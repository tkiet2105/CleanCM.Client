using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

public class Image : BaseAuditableEntity, IAggregateRoot
{
    public string FileName { get; private set; } = string.Empty;   // tên file
    public string Url { get; private set; } = string.Empty;        // link ảnh
    public string? Alt { get; private set; }                       // mô tả alt
    public int SortOrder { get; private set; }                     // thứ tự hiển thị
    public bool IsPrimary { get; private set; }                    // ảnh đại diện

    public Guid ProductId { get; private set; }                    // FK Product

    private Image() : base() { }

    public static Image Create(
        Guid productId,
        string fileName,
        string url,
        string? alt = null,
        int sortOrder = 0,
        bool isPrimary = false)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required", nameof(productId));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName is required", nameof(fileName));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required", nameof(url));

        return new Image
        {
            ProductId = productId,
            FileName = fileName.Trim(),
            Url = url.Trim(),
            Alt = string.IsNullOrWhiteSpace(alt) ? null : alt.Trim(),
            SortOrder = sortOrder,
            IsPrimary = isPrimary
        };
    }

    public void UpdateInfo(string? alt, int? sortOrder = null, bool? isPrimary = null)
    {
        if (alt != null)
            Alt = string.IsNullOrWhiteSpace(alt) ? null : alt.Trim();

        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        if (isPrimary.HasValue)
            IsPrimary = isPrimary.Value;
    }

    public void UpdateFile(string fileName, string url)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName is required", nameof(fileName));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required", nameof(url));

        FileName = fileName.Trim();
        Url = url.Trim();
    }

    public void MoveToProduct(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required", nameof(productId));

        ProductId = productId;
    }

    public void SetPrimary() => IsPrimary = true;
    public void UnsetPrimary() => IsPrimary = false;
}
