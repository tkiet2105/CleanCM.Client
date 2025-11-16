using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Replies)
            .Where(c => c.ProductId == productId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetRepliesAsync(Guid parentCommentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetRootCommentsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Replies)
            .Where(c => c.ProductId == productId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCommentCountAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(c => c.ProductId == productId, cancellationToken);
    }
}