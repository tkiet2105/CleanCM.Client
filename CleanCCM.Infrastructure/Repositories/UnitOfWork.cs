using Microsoft.EntityFrameworkCore.Storage;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Infrastructure.Data;

namespace CleanCCM.Infrastructure.Repositories;

/// <summary>
/// UNIT OF WORK IMPLEMENTATION - Quản lý transaction và save changes
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// UNIT OF WORK PATTERN LÀ GÌ?
/// - Quản lý transaction cho nhiều repositories
/// - Đảm bảo tất cả changes được save cùng lúc
/// - Atomic operations (all or nothing)
/// 
/// VÍ DỤ THỰC TẾ (Banking):
/// 
/// // KHÔNG DÙNG TRANSACTION (XẤU):
/// await _accountRepository.DeductBalance(fromAccount, 1000);
/// await _accountRepository.SaveAsync();  // ✓ Saved
/// 
/// await _accountRepository.AddBalance(toAccount, 1000);
/// await _accountRepository.SaveAsync();  // ✗ FAIL (network error)
/// 
/// // KẾT QUẢ: 1000$ BỊ MẤT! (from account trừ, to account không cộng)
/// 
/// // DÙNG UNIT OF WORK (TỐT):
/// await _accountRepository.DeductBalance(fromAccount, 1000);
/// await _accountRepository.AddBalance(toAccount, 1000);
/// await _unitOfWork.SaveChangesAsync();  // ALL or NOTHING
/// 
/// // Nếu FAIL → ROLLBACK cả 2 operations
/// // Nếu SUCCESS → COMMIT cả 2 operations
/// 
/// ACID PROPERTIES:
/// - Atomicity: All or nothing
/// - Consistency: Data luôn hợp lệ
/// - Isolation: Transactions không ảnh hưởng nhau
/// - Durability: Data được lưu vĩnh viễn sau commit
/// 
/// CÁCH HOẠT ĐỘNG:
/// 
/// 1. BEGIN TRANSACTION (optional - manual control):
///    await _unitOfWork.BeginTransactionAsync();
/// 
/// 2. THỰC HIỆN OPERATIONS:
///    await _userRepository.AddAsync(user);
///    await _orderRepository.AddAsync(order);
/// 
/// 3. COMMIT:
///    await _unitOfWork.CommitTransactionAsync();
/// 
/// HOẶC SIMPLE (tự động transaction):
///    await _userRepository.AddAsync(user);
///    await _orderRepository.AddAsync(order);
///    await _unitOfWork.SaveChangesAsync();  // Auto transaction
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public ITagRepository Tags { get; }
    public IProductCategoryRepository ProductCategories { get; }
    public IProductTagRepository ProductTags { get; }

    // ĐÃ ĐỔI TÊN
    public IReactionRepository Reactions { get; }
    public ICommentRepository Comments { get; }
    public IRatingRepository Ratings { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IProductRepository products,
        ICategoryRepository categories,
        ITagRepository tags,
        IProductCategoryRepository productCategories,
        IProductTagRepository productTags,
        IReactionRepository reactions,
        ICommentRepository comments,
        IRatingRepository ratings)
    {
        _context = context;

        Products = products;
        Categories = categories;
        Tags = tags;
        ProductCategories = productCategories;
        ProductTags = productTags;

        Reactions = reactions;
        Comments = comments;
        Ratings = ratings;
    }
    /// <summary>
    /// LƯU TẤT CẢ THAY ĐỔI
    /// 
    /// GIẢI THÍCH:
    /// - Commit tất cả Add, Update, Remove từ repositories
    /// - Tự động wrap trong transaction nếu chưa có
    /// - Rollback nếu có exception
    /// 
    /// FLOW:
    /// 
    /// 1. EF CORE DETECT CHANGES:
    ///    - Duyệt qua ChangeTracker
    ///    - Tìm entities có State: Added, Modified, Deleted
    /// 
    /// 2. GENERATE SQL:
    ///    - INSERT cho Added
    ///    - UPDATE cho Modified
    ///    - UPDATE (soft delete) hoặc DELETE cho Deleted
    /// 
    /// 3. BEGIN TRANSACTION (nếu chưa có):
    ///    BEGIN TRANSACTION
    /// 
    /// 4. EXECUTE SQL:
    ///    INSERT INTO Users ...
    ///    UPDATE Products ...
    ///    UPDATE Orders SET IsDeleted = 1 ...
    /// 
    /// 5. COMMIT:
    ///    COMMIT TRANSACTION
    /// 
    /// 6. RETURN:
    ///    Return số records affected
    /// 
    /// VÍ DỤ:
    /// 
    /// // Thêm 2 users, update 1 product
    /// await _userRepository.AddAsync(user1);
    /// await _userRepository.AddAsync(user2);
    /// _productRepository.Update(product);
    /// 
    /// var count = await _unitOfWork.SaveChangesAsync();
    /// // count = 3 (2 inserts + 1 update)
    /// 
    /// EXCEPTION HANDLING:
    /// 
    /// try
    /// {
    ///     await _unitOfWork.SaveChangesAsync();
    /// }
    /// catch (DbUpdateException ex)
    /// {
    ///     // Database errors:
    ///     // - Unique constraint violation
    ///     // - Foreign key violation
    ///     // - Connection timeout
    /// }
    /// catch (DbUpdateConcurrencyException ex)
    /// {
    ///     // Concurrency conflicts:
    ///     // - Record đã bị update bởi user khác
    ///     // - Optimistic concurrency check fail
    /// }
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // GỌI DbContext.SaveChangesAsync()
        // - Trigger Interceptors (AuditableEntityInterceptor)
        // - Execute SQL commands
        // - Return số records affected
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// BẮT ĐẦU TRANSACTION (Manual Control)
    /// 
    /// GIẢI THÍCH:
    /// - Tạo transaction thủ công
    /// - Dùng khi cần kiểm soát transaction phức tạp
    /// - Phải gọi Commit hoặc Rollback sau đó
    /// 
    /// KHI NÀO DÙNG?
    /// 
    /// 1. TRANSACTION TRẢI DÀI NHIỀU SaveChanges:
    ///    await _unitOfWork.BeginTransactionAsync();
    ///    
    ///    await _userRepository.AddAsync(user);
    ///    await _unitOfWork.SaveChangesAsync();  // Save 1
    ///    
    ///    await SendEmailAsync(user.Email);       // External operation
    ///    
    ///    await _auditRepository.AddAsync(audit);
    ///    await _unitOfWork.SaveChangesAsync();  // Save 2
    ///    
    ///    await _unitOfWork.CommitTransactionAsync();
    /// 
    /// 2. ROLLBACK GIỮA CHỪNG:
    ///    await _unitOfWork.BeginTransactionAsync();
    ///    
    ///    await _orderRepository.AddAsync(order);
    ///    await _unitOfWork.SaveChangesAsync();
    ///    
    ///    if (inventory.Stock < order.Quantity)
    ///    {
    ///        await _unitOfWork.RollbackTransactionAsync();  // Undo order
    ///        return Error.Business(...);
    ///    }
    ///    
    ///    await _unitOfWork.CommitTransactionAsync();
    /// 
    /// 3. ISOLATION LEVEL:
    ///    // Có thể set isolation level (advanced)
    ///    await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    /// 
    /// SQL:
    /// BEGIN TRANSACTION
    /// -- Operations here
    /// -- COMMIT or ROLLBACK later
    /// 
    /// LƯU Ý:
    /// - PHẢI gọi Commit hoặc Rollback
    /// - Nếu không → connection leak, deadlock
    /// - Dùng try-finally để cleanup
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // TẠO TRANSACTION
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // LƯU TRANSACTION OBJECT để commit/rollback sau
    }

    /// <summary>
    /// COMMIT TRANSACTION - Xác nhận tất cả thay đổi
    /// 
    /// GIẢI THÍCH:
    /// - Commit transaction đã begin
    /// - Lưu vĩnh viễn tất cả changes
    /// - Cleanup transaction object
    /// 
    /// FLOW:
    /// 
    /// 1. SAVE CHANGES (nếu có):
    ///    await SaveChangesAsync();
    /// 
    /// 2. COMMIT TRANSACTION:
    ///    await _transaction.CommitAsync();
    /// 
    /// 3. CLEANUP:
    ///    await _transaction.DisposeAsync();
    ///    _transaction = null;
    /// 
    /// VÍ DỤ:
    /// 
    /// await _unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     // Operations
    ///     await _userRepository.AddAsync(user);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     await _orderRepository.AddAsync(order);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // All OK → Commit
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// 
    /// SQL:
    /// BEGIN TRANSACTION
    ///     INSERT INTO Users ...
    ///     INSERT INTO Orders ...
    /// COMMIT TRANSACTION  ← CommitTransactionAsync()
    /// 
    /// SAU COMMIT:
    /// - Data được lưu vĩnh viễn
    /// - Không thể rollback
    /// - Transactions khác có thể đọc data
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // SAVE CHANGES trước khi commit
            await SaveChangesAsync(cancellationToken);

            // COMMIT TRANSACTION
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            // NẾU CÓ LỖI → ROLLBACK
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            // CLEANUP (luôn chạy)
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// ROLLBACK TRANSACTION - Hủy bỏ tất cả thay đổi
    /// 
    /// GIẢI THÍCH:
    /// - Hủy transaction đã begin
    /// - Tất cả changes bị undo
    /// - Database trở về trạng thái trước transaction
    /// 
    /// KHI NÀO DÙNG?
    /// - Exception xảy ra
    /// - Business logic validation fail
    /// - Muốn hủy operation
    /// 
    /// VÍ DỤ:
    /// 
    /// await _unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     // Create order
    ///     await _orderRepository.AddAsync(order);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     // Check stock
    ///     if (product.Stock < order.Quantity)
    ///     {
    ///         // Không đủ hàng → Rollback order
    ///         await _unitOfWork.RollbackTransactionAsync();
    ///         return Error.Business(..., "Out of stock");
    ///     }
    ///     
    ///     // Update stock
    ///     product.DecreaseStock(order.Quantity);
    ///     await _unitOfWork.SaveChangesAsync();
    ///     
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch (Exception)
    /// {
    ///     // Exception → Rollback
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// 
    /// SQL:
    /// BEGIN TRANSACTION
    ///     INSERT INTO Orders ...  ← Đã insert
    ///     -- Check stock fail
    /// ROLLBACK TRANSACTION  ← RollbackTransactionAsync()
    /// -- Order bị xóa, như chưa có gì xảy ra
    /// 
    /// SAU ROLLBACK:
    /// - Tất cả changes bị hủy
    /// - Database trở về trạng thái ban đầu
    /// - Có thể begin transaction mới
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            // ROLLBACK TRANSACTION
            await _transaction.RollbackAsync(cancellationToken);

            // CLEANUP
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// DISPOSE - Cleanup resources
    /// 
    /// GIẢI THÍCH:
    /// - Implement IDisposable
    /// - Cleanup transaction nếu còn
    /// - Cleanup context
    /// 
    /// CÁCH DÙNG:
    /// 
    /// // Manual dispose
    /// _unitOfWork.Dispose();
    /// 
    /// // Using statement (tự động dispose)
    /// using (var unitOfWork = new UnitOfWork(context))
    /// {
    ///     // Operations
    /// }  // Tự động gọi Dispose()
    /// 
    /// LƯU Ý:
    /// - DI container tự động dispose
    /// - Scoped lifetime → dispose cuối request
    /// - Không cần manual dispose trong ASP.NET Core
    /// </summary>
    public void Dispose()
    {
        // DISPOSE TRANSACTION
        _transaction?.Dispose();

        // DISPOSE CONTEXT
        _context.Dispose();
    }
}