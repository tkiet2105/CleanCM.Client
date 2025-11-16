using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
// ==================== ICategoryRepository ====================
// Location: CleanCCM.Application/Common/Interfaces/ICategoryRepository.cs

/// <summary>
/// Sử dụng: Repository cho Category với các query đặc thù
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetByDisplayOrderAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsSlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}