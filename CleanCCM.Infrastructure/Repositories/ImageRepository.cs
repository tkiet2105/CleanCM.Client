using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;   // đổi lại namespace đúng của bạn
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class ImageRepository : Repository<Image>, IImageRepository
{
    public ImageRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Lấy list ảnh theo Product, sort theo SortOrder (asc).
    /// </summary>
    public async Task<List<Image>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
            return new List<Image>();

        return await _context.Images
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.SortOrder)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy ảnh đại diện cho Product.
    /// </summary>
    public async Task<Image?> GetPrimaryImageAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
            return null;

        return await _context.Images
            .Where(x => x.ProductId == productId && x.IsPrimary)
            .OrderBy(x => x.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra product có ảnh đại diện chưa.
    /// </summary>
    public async Task<bool> HasPrimaryAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
            return false;

        return await _context.Images
            .AnyAsync(x => x.ProductId == productId && x.IsPrimary, cancellationToken);
    }
}
