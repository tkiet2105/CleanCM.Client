using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
/// <summary>
/// Sử dụng: Repository cho Reaction
/// </summary>
public interface IReactionRepository : IRepository<Reaction>
{
    Task<Reaction?> GetByProductAndUserAsync(Guid productId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reaction>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reaction>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> HasUserReactedAsync(Guid productId, string userId, CancellationToken cancellationToken = default);
}
