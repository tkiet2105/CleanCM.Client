using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class ProductTagRepository : IProductTagRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ProductTag> _dbSet;

    public ProductTagRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ProductTag>();
    }

    public async Task<ProductTag?> GetByIdAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Product)
            .Include(pt => pt.Tag)
            .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
    }

    public async Task<IEnumerable<ProductTag>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Tag)
            .Where(pt => pt.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductTag>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pt => pt.Product)
            .Where(pt => pt.TagId == tagId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductTag productTag, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(productTag, cancellationToken);
    }

    public Task RemoveAsync(ProductTag productTag)
    {
        _dbSet.Remove(productTag);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
    }
}