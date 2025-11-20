using CleanCCM.Domain.Common;

namespace CleanCCM.Domain.Entities;

public class Address : BaseAuditableEntity, IAggregateRoot
{
    public string Line1 { get; private set; } = string.Empty;   // Số nhà, đường
    public string? Line2 { get; private set; }                  // Thêm (tầng, block…)
    public string City { get; private set; } = string.Empty;    // Tỉnh / thành phố 
    public string District { get; private set; } = string.Empty;// Quận , huyện
    public string Ward { get; private set; } = string.Empty;    // Phường xã
    public string Country { get; private set; } = "Vietnam";
    public bool IsPrimary { get; private set; }                 // Địa chỉ chính

    // FK (tuỳ ngữ cảnh mà dùng 1 trong 2 hoặc cả 2)
    public Guid? ProductId { get; private set; }
    public string? UserId { get; private set; }

    // EF cần
    private Address() { }

    #region Factory

    public static Address Create(
        string line1,
        string city,
        string district,
        string ward,
        string? line2 = null,
        Guid? productId = null,
        string? userId = null,
        bool isPrimary = false,
        string country = "Vietnam")
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Line1 is required", nameof(line1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District is required", nameof(district));
        if (string.IsNullOrWhiteSpace(ward))
            throw new ArgumentException("Ward is required", nameof(ward));

        return new Address
        {
            ProductId = productId,
            UserId = userId,
            Line1 = line1.Trim(),
            Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim(),
            City = city.Trim(),
            District = district.Trim(),
            Ward = ward.Trim(),
            Country = string.IsNullOrWhiteSpace(country) ? "Vietnam" : country.Trim(),
            IsPrimary = isPrimary
        };
    }

    #endregion

    #region Update methods

    /// <summary>
    /// Cập nhật toàn bộ thông tin địa chỉ (trừ FK).
    /// </summary>
    public void UpdateDetails(
        string line1,
        string city,
        string district,
        string ward,
        string? line2 = null,
        string? country = null)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Line1 is required", nameof(line1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District is required", nameof(district));
        if (string.IsNullOrWhiteSpace(ward))
            throw new ArgumentException("Ward is required", nameof(ward));

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        City = city.Trim();
        District = district.Trim();
        Ward = ward.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? Country : country!.Trim();
    }

    /// <summary>
    /// Chỉ update phần location chính (City/District/Ward).
    /// </summary>
    public void UpdateLocation(
        string city,
        string district,
        string ward)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District is required", nameof(district));
        if (string.IsNullOrWhiteSpace(ward))
            throw new ArgumentException("Ward is required", nameof(ward));

        City = city.Trim();
        District = district.Trim();
        Ward = ward.Trim();
    }

    /// <summary>
    /// Update Line1 + Line2.
    /// </summary>
    public void UpdateLines(string line1, string? line2 = null)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Line1 is required", nameof(line1));

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
    }

    /// <summary>
    /// Gán/đổi địa chỉ này cho 1 product.
    /// </summary>
    public void AssignToProduct(Guid? productId)
    {
        ProductId = productId;
    }

    /// <summary>
    /// Gán/đổi địa chỉ này cho 1 user.
    /// </summary>
    public void AssignToUser(string? userId)
    {
        UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
    }

    #endregion

    #region Primary helpers

    public void SetPrimary()
    {
        IsPrimary = true;
    }

    public void UnsetPrimary()
    {
        IsPrimary = false;
    }

    public void TogglePrimary()
    {
        IsPrimary = !IsPrimary;
    }

    #endregion
}
