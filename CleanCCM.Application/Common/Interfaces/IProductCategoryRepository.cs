using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
// ==================== IProductCategoryRepository ====================
// Location: CleanCCM.Application/Common/Interfaces/IProductCategoryRepository.cs

/// <summary>
/// Sử dụng: Repository cho bảng trung gian ProductCategory
/// </summary>
public interface IProductCategoryRepository
{
    Task<ProductCategory?> GetByIdAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductCategory>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task RemoveAsync(ProductCategory productCategory);
    Task<bool> ExistsAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default);
}