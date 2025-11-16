using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class RatingRepository : Repository<Rating>, IRatingRepository
{
    public RatingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Rating?> GetByProductAndUserAsync(Guid productId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Rating>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rating>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var ratings = await _dbSet
            .Where(r => r.ProductId == productId)
            .ToListAsync(cancellationToken);

        return ratings.Any() ? ratings.Average(r => r.Score) : 0;
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var distribution = await _dbSet
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.Score)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

        for (int i = 1; i <= 5; i++)
        {
            if (!distribution.ContainsKey(i))
                distribution[i] = 0;
        }

        return distribution;
    }

    public async Task<bool> HasUserRatedAsync(Guid productId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId, cancellationToken);
    }
}