
namespace CleanCCM.Application.Common.Interfaces;

/// <summary>
/// DATABASE CONTEXT INTERFACE - Abstraction của DbContext
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// TẠI SAO CẦN INTERFACE CHO DbContext?
/// 
/// 1. DEPENDENCY INVERSION PRINCIPLE (DIP):
///    - Application không phụ thuộc vào Infrastructure
///    - Application chỉ phụ thuộc vào Interface
///    - Infrastructure implement Interface
/// 
/// 2. TESTABILITY:
///    - Dễ mock trong unit tests
///    - Không cần database thật khi test
///    
/// 3. CLEAN ARCHITECTURE:
///    Application ← Interface ← Infrastructure
///    (không biết)    (biết)    (implement)
/// 
/// KIẾN TRÚC:
/// 
/// CleanCCM.Application/
/// ├── Interfaces/
/// │   └── IApplicationDbContext.cs ← Interface (tầng Application)
/// 
/// CleanCCM.Infrastructure/
/// ├── Data/
/// │   └── ApplicationDbContext.cs ← Implementation (tầng Infrastructure)
/// 
/// VÍ DỤ KHÔNG DÙNG INTERFACE (XẤU):
/// 
/// // Handler phụ thuộc trực tiếp vào DbContext
/// public class GetUsersQueryHandler
/// {
///     private readonly ApplicationDbContext _context; // COUPLING!
///     
///     public async Task<List<User>> Handle()
///     {
///         return await _context.Users.ToListAsync();
///     }
/// }
/// 
/// VẤN ĐỀ:
/// - Application phụ thuộc Infrastructure
/// - Vi phạm Clean Architecture
/// - Khó test (cần database thật)
/// 
/// VÍ DỤ DÙNG INTERFACE (TỐT):
/// 
/// // Handler chỉ phụ thuộc Interface
/// public class GetUsersQueryHandler
/// {
///     private readonly IApplicationDbContext _context; // ABSTRACTION!
///     
///     public async Task<List<User>> Handle()
///     {
///         return await _context.Users.ToListAsync();
///     }
/// }
/// 
/// ƯU ĐIỂM:
/// - Application không biết implementation
/// - Dễ test (mock IApplicationDbContext)
/// - Tuân thủ Clean Architecture
/// 
/// KHI NÀO DÙNG DbContext vs Repository?
/// 
/// DÙNG Repository KHI:
/// - CRUD operations đơn giản
/// - Generic operations
/// - Ví dụ: GetById, Add, Update, Delete
/// 
/// DÙNG DbContext KHI:
/// - Complex queries với LINQ
/// - Join nhiều bảng
/// - Raw SQL
/// - Ví dụ: Statistics, Reports, Complex filters
/// 
/// VÍ DỤ:
/// 
/// // Simple - Dùng Repository
/// var user = await _userRepository.GetByIdAsync(userId);
/// 
/// // Complex - Dùng DbContext
/// var stats = await _context.Orders
///     .Include(o => o.Items)
///     .Where(o => o.CreatedAt >= startDate)
///     .GroupBy(o => o.Status)
///     .Select(g => new { Status = g.Key, Count = g.Count() })
///     .ToListAsync();
/// </summary>
public interface IApplicationDbContext
{
    // ==================== DbSets - ĐỊNH NGHĨA Ở ĐÂY ====================

    /// <summary>
    /// DbSet cho entity User
    /// 
    /// GIẢI THÍCH:
    /// - Mỗi entity có 1 DbSet
    /// - DbSet đại diện cho bảng trong database
    /// - Dùng để query và manipulate data
    /// 
    /// VÍ DỤ THÊM DbSet:
    /// 
    /// // Trong Interface
    /// public interface IApplicationDbContext
    /// {
    ///     DbSet<User> Users { get; }
    ///     DbSet<Product> Products { get; }
    ///     DbSet<Order> Orders { get; }
    ///     DbSet<Category> Categories { get; }
    /// }
    /// 
    /// // Trong Implementation
    /// public class ApplicationDbContext : DbContext, IApplicationDbContext
    /// {
    ///     public DbSet<User> Users => Set<User>();
    ///     public DbSet<Product> Products => Set<Product>();
    ///     public DbSet<Order> Orders => Set<Order>();
    ///     public DbSet<Category> Categories => Set<Category>();
    /// }
    /// 
    /// CÁCH DÙNG:
    /// 
    /// // Query users
    /// var users = await _context.Users
    ///     .Where(u => u.IsActive)
    ///     .ToListAsync();
    /// 
    /// // Join queries
    /// var ordersWithUser = await _context.Orders
    ///     .Include(o => o.User)
    ///     .Where(o => o.Status == OrderStatus.Pending)
    ///     .ToListAsync();
    /// 
    /// // Complex aggregation
    /// var stats = await _context.Products
    ///     .GroupBy(p => p.CategoryId)
    ///     .Select(g => new
    ///     {
    ///         CategoryId = g.Key,
    ///         ProductCount = g.Count(),
    ///         TotalValue = g.Sum(p => p.Price * p.Stock)
    ///     })
    ///     .ToListAsync();
    /// 
    /// LƯU Ý:
    /// - DbSet<User> Users: LUÔN LUÔN dùng số NHIỀU (Users, Products, Orders)
    /// - Convention trong EF Core
    /// - Table name cũng là số nhiều
    /// </summary>
    // DbSet<User> Users { get; }
    // DbSet<Product> Products { get; }
    // DbSet<Order> Orders { get; }
    // ... thêm các DbSet khác ở đây

    // LƯU Ý CHO JUNIOR:
    // File này là INTERFACE nên chỉ KHAI BÁO
    // KHÔNG IMPLEMENT ở đây
    // Implementation ở CleanCCM.Infrastructure/Data/ApplicationDbContext.cs

    // ==================== SaveChangesAsync ====================

    /// <summary>
    /// LƯU TẤT CẢ THAY ĐỔI vào database
    /// 
    /// GIẢI THÍCH:
    /// - Method quan trọng nhất của DbContext
    /// - Commit tất cả Add, Update, Remove vào DB
    /// - Tự động wrap trong transaction
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 
    /// 1. TRACK CHANGES:
    ///    var user = new User();
    ///    _context.Users.Add(user);        // State = Added
    ///    
    ///    var product = await _context.Products.FindAsync(id);
    ///    product.Name = "New Name";       // State = Modified
    ///    
    ///    _context.Products.Remove(product); // State = Deleted
    /// 
    /// 2. GENERATE SQL:
    ///    await _context.SaveChangesAsync();
    ///    
    ///    → EF Core generate:
    ///    BEGIN TRANSACTION
    ///        INSERT INTO Users (...) VALUES (...)
    ///        UPDATE Products SET Name = ... WHERE Id = ...
    ///        DELETE FROM Products WHERE Id = ...
    ///    COMMIT TRANSACTION
    /// 
    /// 3. EXECUTE:
    ///    - Gửi SQL commands tới database
    ///    - Database execute trong transaction
    ///    - Nếu thành công → commit
    ///    - Nếu fail → rollback
    /// 
    /// VÍ DỤ:
    /// 
    /// // Thêm mới
    /// var user = User.Create(...);
    /// _context.Users.Add(user);
    /// await _context.SaveChangesAsync();
    /// 
    /// // Cập nhật
    /// var product = await _context.Products.FindAsync(id);
    /// product.UpdatePrice(newPrice);
    /// await _context.SaveChangesAsync(); // EF tự detect changes
    /// 
    /// // Xóa
    /// var order = await _context.Orders.FindAsync(id);
    /// _context.Orders.Remove(order);
    /// await _context.SaveChangesAsync();
    /// 
    /// RETURN VALUE:
    /// - int: Số records đã được modified
    /// 
    /// VÍ DỤ:
    /// _context.Users.Add(user1);
    /// _context.Users.Add(user2);
    /// _context.Products.Remove(product);
    /// 
    /// var count = await _context.SaveChangesAsync();
    /// // count = 3 (2 inserts + 1 delete)
    /// 
    /// EXCEPTION HANDLING:
    /// 
    /// try
    /// {
    ///     await _context.SaveChangesAsync();
    /// }
    /// catch (DbUpdateException ex)
    /// {
    ///     // Database errors
    ///     // - Unique constraint violation
    ///     // - Foreign key violation
    ///     // - Connection timeout
    /// }
    /// catch (DbUpdateConcurrencyException ex)
    /// {
    ///     // Concurrency conflicts
    ///     // - Record đã bị xóa bởi user khác
    ///     // - Record đã bị update bởi user khác
    /// }
    /// 
    /// PERFORMANCE:
    /// - Batch multiple operations
    /// - Một lần SaveChanges cho nhiều entities
    /// - Tránh save trong loop
    /// 
    /// // XẤU - Save trong loop
    /// foreach (var user in users)
    /// {
    ///     _context.Users.Add(user);
    ///     await _context.SaveChangesAsync(); // 100 DB calls!
    /// }
    /// 
    /// // TỐT - Batch save
    /// foreach (var user in users)
    /// {
    ///     _context.Users.Add(user);
    /// }
    /// await _context.SaveChangesAsync(); // 1 DB call
    /// </summary>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <returns>Số lượng records đã được save</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}