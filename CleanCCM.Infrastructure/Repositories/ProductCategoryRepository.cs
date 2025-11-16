using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ProductCategory> _dbSet;

    public ProductCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ProductCategory>();
    }

    public async Task<ProductCategory?> GetByIdAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pc => pc.Product)
            .Include(pc => pc.Category)
            .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId, cancellationToken);
    }

    public async Task<IEnumerable<ProductCategory>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pc => pc.Category)
            .Where(pc => pc.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductCategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pc => pc.Product)
            .Where(pc => pc.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(productCategory, cancellationToken);
    }

    public Task RemoveAsync(ProductCategory productCategory)
    {
        _dbSet.Remove(productCategory);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId, cancellationToken);
    }
}