using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByTagAsync(
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .Where(p => p.ProductTags.Any(pt => pt.TagId == tagId))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsPublished)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(searchTerm) ||
                       (p.Description != null && p.Description.Contains(searchTerm)))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Reactions)
            .Include(p => p.Ratings)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetMostViewedAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Reactions)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.Reactions.Count)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetMostLikedAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Reactions)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.Reactions.Count(r => r.Type == ReactionType.Like))
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetTopRatedAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Ratings)
            .Where(p => p.IsPublished && p.Ratings.Any())
            .OrderByDescending(p => p.Ratings.Average(r => r.Score))
            .ThenByDescending(p => p.Ratings.Count)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<bool> IsSlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Slug == slug);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}