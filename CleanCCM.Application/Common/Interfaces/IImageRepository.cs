using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
// ==================== IImageRepository ====================
// Location: CleanCCM.Application/Common/Interfaces/IImageRepository.cs

/// <summary>
/// Sử dụng: Repository cho Image với các query đặc thù
/// </summary>

public interface IImageRepository : IRepository<Image>
{
    /// <summary>
    /// Lấy danh sách ảnh theo ProductId, sort theo SortOrder.
    /// </summary>
    Task<List<Image>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy ảnh đại diện (IsPrimary = true) cho product.
    /// </summary>
    Task<Image?> GetPrimaryImageAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra product có ảnh đại diện chưa.
    /// </summary>
    Task<bool> HasPrimaryAsync(Guid productId, CancellationToken cancellationToken = default);
}