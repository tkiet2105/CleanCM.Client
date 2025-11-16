using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
// ==================== ITagRepository ====================
// Location: CleanCCM.Application/Common/Interfaces/ITagRepository.cs

/// <summary>
/// Sử dụng: Repository cho Tag
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetActiveTagsAsync(CancellationToken cancellationToken = default);
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsSlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetPopularTagsAsync(int take = 10, CancellationToken cancellationToken = default);
}