using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Common.Interfaces;

/// <summary>
/// Sử dụng: Repository cho Comment
/// </summary>
public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetRepliesAsync(Guid parentCommentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetRootCommentsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<int> GetCommentCountAsync(Guid productId, CancellationToken cancellationToken = default);
}
