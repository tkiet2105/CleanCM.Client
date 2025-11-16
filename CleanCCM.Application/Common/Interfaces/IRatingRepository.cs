using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;
/// <summary>
/// Sử dụng: Repository cho Rating
/// </summary>
public interface IRatingRepository : IRepository<Rating>
{
    Task<Rating?> GetByProductAndUserAsync(Guid productId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rating>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Rating>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<double> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> HasUserRatedAsync(Guid productId, string userId, CancellationToken cancellationToken = default);
}