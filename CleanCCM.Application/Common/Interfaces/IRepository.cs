using System.Linq.Expressions;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common;

namespace CleanCCM.Application.Common.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    // ==================== READ OPERATIONS (QUERIES) ====================

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    // ==================== WRITE OPERATIONS (COMMANDS) ====================

    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default);

    void Update(T entity);

    void UpdateRange(IEnumerable<T> entities);

    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);

    // ==================== PAGINATION ====================

    Task<PaginatedList<T>> GetPaginatedAsync(
        PaginationRequest request,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}