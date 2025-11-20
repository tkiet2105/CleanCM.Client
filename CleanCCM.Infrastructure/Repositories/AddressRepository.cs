using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data; // hoặc namespace ApplicationDbContext của bạn
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class AddressRepository : Repository<Address>, IAddressRepository
{
    public AddressRepository(ApplicationDbContext context) : base(context)
    {
    }

    // ==================== USER ====================

    public async Task<List<Address>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<Address>();

        return await _context.Addresses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Address?> GetPrimaryByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return await _context.Addresses
            .Where(x => x.UserId == userId && x.IsPrimary)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UserHasPrimaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return await _context.Addresses
            .AnyAsync(x => x.UserId == userId && x.IsPrimary, cancellationToken);
    }

    // ==================== PRODUCT ====================

    public async Task<Address?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
            return null;

        return await _context.Addresses
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ProductHasAddressAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
            return false;

        return await _context.Addresses
            .AnyAsync(x => x.ProductId == productId, cancellationToken);
    }
}
