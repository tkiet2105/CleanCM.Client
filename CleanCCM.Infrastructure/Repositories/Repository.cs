using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common;
using CleanCCM.Infrastructure.Data;

namespace CleanCCM.Infrastructure.Repositories;

/// <summary>
/// GENERIC REPOSITORY IMPLEMENTATION
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// REPOSITORY PATTERN LÀ GÌ?
/// - Abstraction layer giữa Business Logic và Data Access
/// - Che giấu EF Core implementation details
/// - Cung cấp clean API cho CRUD operations
/// 
/// GENERIC REPOSITORY:
/// - Một repository cho MỌI entity
/// - Không cần UserRepository, ProductRepository...
/// - Giảm code duplicate
/// 
/// VÍ DỤ SỬ DỤNG:
/// 
/// // Inject repository
/// private readonly IRepository<User> _userRepository;
/// private readonly IRepository<Product> _productRepository;
/// 
/// // Query
/// var user = await _userRepository.GetByIdAsync(userId);
/// var products = await _productRepository.FindAsync(p => p.Price > 100);
/// 
/// // Insert
/// await _userRepository.AddAsync(user);
/// await _unitOfWork.SaveChangesAsync();
/// 
/// // Update
/// _userRepository.Update(user);
/// await _unitOfWork.SaveChangesAsync();
/// 
/// // Delete
/// _userRepository.Remove(user);
/// await _unitOfWork.SaveChangesAsync();
/// 
/// TẠI SAO KHÔNG TRỰC TIẾP DÙNG DbContext?
/// 
/// 1. ABSTRACTION:
///    - Business logic không phụ thuộc EF Core
///    - Có thể đổi ORM (Dapper, NHibernate...)
/// 
/// 2. TESTABILITY:
///    - Dễ mock IRepository
///    - Unit test không cần database
/// 
/// 3. CONSISTENCY:
///    - Pagination, sorting logic tập trung
///    - Soft delete tự động
///    - Reusable
/// </summary>
/// <typeparam name="T">Entity type (User, Product, Order...)</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// DATABASE CONTEXT
    /// </summary>
    protected readonly ApplicationDbContext _context;

    /// <summary>
    /// DbSet<T> cho entity type này
    /// 
    /// VÍ DỤ:
    /// Repository<User> → _dbSet = context.Users
    /// Repository<Product> → _dbSet = context.Products
    /// </summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Constructor - Inject DbContext
    /// </summary>
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ==================== READ OPERATIONS ====================

    /// <summary>
    /// TÌM ENTITY THEO ID
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.GetByIdAsync(userId);
    /// if (user == null)
    ///     return Error.NotFound(...);
    /// 
    /// SQL:
    /// SELECT * FROM Users WHERE Id = @id AND IsDeleted = 0
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// LẤY TẤT CẢ ENTITIES
    /// 
    /// ⚠️ CẢNH BÁO:
    /// - KHÔNG dùng với bảng lớn (>1000 records)
    /// - Dùng GetPaginatedAsync thay thế
    /// 
    /// VÍ DỤ:
    /// var allUsers = await _repository.GetAllAsync();
    /// 
    /// SQL:
    /// SELECT * FROM Users WHERE IsDeleted = 0
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// TÌM ENTITIES THEO PREDICATE
    /// 
    /// VÍ DỤ:
    /// var activeUsers = await _repository.FindAsync(u => u.IsActive);
    /// var expensiveProducts = await _repository.FindAsync(p => p.Price > 1000);
    /// 
    /// SQL:
    /// SELECT * FROM Users WHERE IsActive = 1 AND IsDeleted = 0
    /// SELECT * FROM Products WHERE Price > 1000 AND IsDeleted = 0
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// LẤY ENTITY ĐẦU TIÊN HOẶC NULL
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.FirstOrDefaultAsync(u => u.Email == email);
    /// 
    /// SQL:
    /// SELECT TOP 1 * FROM Users WHERE Email = @email AND IsDeleted = 0
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// KIỂM TRA TỒN TẠI
    /// 
    /// VÍ DỤ:
    /// var emailExists = await _repository.AnyAsync(u => u.Email == email);
    /// if (emailExists)
    ///     return Error.Conflict(...);
    /// 
    /// SQL:
    /// SELECT CASE WHEN EXISTS (
    ///     SELECT 1 FROM Users WHERE Email = @email AND IsDeleted = 0
    /// ) THEN 1 ELSE 0 END
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// ĐẾM SỐ LƯỢNG
    /// 
    /// VÍ DỤ:
    /// var totalUsers = await _repository.CountAsync();
    /// var activeUsers = await _repository.CountAsync(u => u.IsActive);
    /// 
    /// SQL:
    /// SELECT COUNT(*) FROM Users WHERE IsDeleted = 0
    /// SELECT COUNT(*) FROM Users WHERE IsActive = 1 AND IsDeleted = 0
    /// </summary>
    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.CountAsync(cancellationToken);

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    // ==================== WRITE OPERATIONS ====================

    /// <summary>
    /// THÊM ENTITY
    /// 
    /// LƯU Ý: Chỉ ADD vào context, CHƯA SAVE vào DB
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// THÊM NHIỀU ENTITIES
    /// </summary>
    public virtual async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        var enumerable = entities.ToList();
        await _dbSet.AddRangeAsync(enumerable, cancellationToken);
        return enumerable;
    }

    /// <summary>
    /// CẬP NHẬT ENTITY
    /// 
    /// LƯU Ý: Mark state = Modified, CHƯA UPDATE vào DB
    /// </summary>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// CẬP NHẬT NHIỀU ENTITIES
    /// </summary>
    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// XÓA ENTITY (SOFT DELETE)
    /// 
    /// LƯU Ý: 
    /// - Nếu entity kế thừa BaseAuditableEntity → Soft delete
    /// - Interceptor sẽ set IsDeleted = true
    /// </summary>
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// XÓA NHIỀU ENTITIES
    /// </summary>
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    // ==================== PAGINATION ====================

    /// <summary>
    /// LẤY DANH SÁCH CÓ PHÂN TRANG
    /// 
    /// VÍ DỤ:
    /// var request = new PaginationRequest
    /// {
    ///     PageNumber = 2,
    ///     PageSize = 10,
    ///     SortBy = "CreatedAt",
    ///     SortDescending = true,
    ///     SearchTerm = "john"
    /// };
    /// 
    /// var result = await _repository.GetPaginatedAsync(
    ///     request,
    ///     predicate: u => u.IsActive
    /// );
    /// 
    /// // result.Items: 10 users của trang 2
    /// // result.TotalCount: Tổng số users
    /// // result.TotalPages: Tổng số trang
    /// </summary>
    public virtual async Task<PaginatedList<T>> GetPaginatedAsync(
        PaginationRequest request,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // APPLY PREDICATE (Filter)
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // SEARCH (nếu có SearchTerm)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // TODO: Implement search logic
            // Phụ thuộc vào entity properties
            // Ví dụ với User:
            // query = query.Where(u => 
            //     u.Email.Contains(request.SearchTerm) ||
            //     u.UserName.Contains(request.SearchTerm)
            // );
        }

        // COUNT TOTAL (trước khi pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // SORTING
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            query = ApplySorting(query, request.SortBy, request.SortDescending);
        }

        // PAGINATION (Skip + Take)
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, totalCount, request.PageNumber, request.PageSize);
    }

    /// <summary>
    /// APPLY SORTING (Dynamic)
    /// 
    /// VÍ DỤ:
    /// SortBy = "CreatedAt", SortDescending = true
    /// → ORDER BY CreatedAt DESC
    /// </summary>
    private IQueryable<T> ApplySorting(IQueryable<T> query, string sortBy, bool descending)
    {
        // BUILD EXPRESSION DYNAMICALLY
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}