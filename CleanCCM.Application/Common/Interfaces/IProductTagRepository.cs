using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;

// ==================== IProductTagRepository ====================
// Location: CleanCCM.Application/Common/Interfaces/IProductTagRepository.cs

/// <summary>
/// Sử dụng: Repository cho bảng trung gian ProductTag
/// </summary>
public interface IProductTagRepository
{
    Task<ProductTag?> GetByIdAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductTag>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductTag>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductTag productTag, CancellationToken cancellationToken = default);
    Task RemoveAsync(ProductTag productTag);
    Task<bool> ExistsAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default);
}
