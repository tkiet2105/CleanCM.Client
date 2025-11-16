using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class ReactionRepository : Repository<Reaction>, IReactionRepository
{
    public ReactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Reaction?> GetByProductAndUserAsync(Guid productId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Reaction>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reaction>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.Type)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<bool> HasUserReactedAsync(Guid productId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
    }
}