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
            .OrderBy(p => p.DisplayOrder)
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
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(searchTerm) ||
                       (p.Description != null && p.Description.Contains(searchTerm)) ||
                       (p.SKU != null && p.SKU.Contains(searchTerm)))
            .OrderBy(p => p.DisplayOrder)
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
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetMostViewedAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.ViewCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetMostLikedAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.LikeCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetTopRatedAsync(
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive && p.RatingCount >= 5)
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.RatingCount)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(productId, cancellationToken);
        if (product != null)
        {
            product.ViewCount++;
            await UpdateAsync(product, cancellationToken);
        }
    }

    public async Task RecalculateRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(productId, cancellationToken);
        if (product == null) return;

        var ratings = await _context.ProductRatings
            .Where(r => r.ProductId == productId)
            .ToListAsync(cancellationToken);

        product.RatingCount = ratings.Count;
        product.AverageRating = ratings.Any()
            ? (decimal?)ratings.Average(r => r.RatingValue)
            : null;

        await UpdateAsync(product, cancellationToken);
    }

    public async Task UpdateEngagementCountsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(productId, cancellationToken);
        if (product == null) return;

        // Recalculate từ ProductReaction
        var reactions = await _context.ProductReactions
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.ReactionType)
            .Select(g => new { Type = g.Key, Count = g.Count(r => r.IsActive) })
            .ToListAsync(cancellationToken);

        product.ViewCount = reactions.FirstOrDefault(r => r.Type == ReactionType.View)?.Count ?? 0;
        product.LikeCount = reactions.FirstOrDefault(r => r.Type == ReactionType.Like)?.Count ?? 0;
        product.LoveCount = reactions.FirstOrDefault(r => r.Type == ReactionType.Love)?.Count ?? 0;
        product.DislikeCount = reactions.FirstOrDefault(r => r.Type == ReactionType.Dislike)?.Count ?? 0;
        product.BookmarkCount = reactions.FirstOrDefault(r => r.Type == ReactionType.Bookmark)?.Count ?? 0;
        product.ShareCount = reactions.FirstOrDefault(r => r.Type == ReactionType.Share)?.Count ?? 0;

        // Comment count
        product.CommentCount = await _context.ProductComments
            .CountAsync(c => c.ProductId == productId, cancellationToken);

        await UpdateAsync(product, cancellationToken);
    }
}