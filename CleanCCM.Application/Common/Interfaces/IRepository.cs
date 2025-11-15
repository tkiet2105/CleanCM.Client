using System.Linq.Expressions;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common;

namespace CleanCCM.Application.Common.Interfaces;

/// <summary>
/// REPOSITORY PATTERN - Interface cho truy xuất dữ liệu
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// REPOSITORY PATTERN LÀ GÌ?
/// - Abstraction layer giữa Business Logic và Data Access
/// - Che giấu chi tiết implementation (SQL, NoSQL, File...)
/// - Cung cấp API đơn giản để CRUD operations
/// 
/// TẠI SAO DÙNG REPOSITORY?
/// 
/// 1. SEPARATION OF CONCERNS:
///    Business Logic không cần biết dùng SQL hay NoSQL
///    Chỉ cần gọi repository methods
/// 
/// 2. TESTABILITY:
///    Dễ mock repository trong unit tests
///    Không cần database thật khi test
/// 
/// 3. MAINTAINABILITY:
///    Đổi database engine? Chỉ cần đổi implementation
///    Business logic KHÔNG thay đổi
/// 
/// VÍ DỤ:
/// 
/// KHÔNG DÙNG REPOSITORY (xấu):
/// public class UserService
/// {
///     private readonly DbContext _context;
///     
///     public async Task<User> GetUser(Guid id)
///     {
///         // Business logic BỊ COUPLING với EF Core
///         return await _context.Users.FindAsync(id);
///     }
/// }
/// 
/// DÙNG REPOSITORY (tốt):
/// public class UserService
/// {
///     private readonly IRepository<User> _repository;
///     
///     public async Task<User> GetUser(Guid id)
///     {
///         // Business logic KHÔNG biết implementation
///         return await _repository.GetByIdAsync(id);
///     }
/// }
/// 
/// GENERIC REPOSITORY:
/// - IRepository<T> generic → Tái sử dụng cho mọi entity
/// - Không cần IUserRepository, IProductRepository...
/// - Giảm code duplicate
/// </summary>
/// <typeparam name="T">Entity type (User, Product, Order...)</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // ==================== READ OPERATIONS (QUERIES) ====================

    /// <summary>
    /// TÌM ENTITY THEO ID
    /// 
    /// VÍ DỤ:
    /// var user = await _userRepository.GetByIdAsync(userId);
    /// if (user == null)
    ///     return Error.NotFound(...);
    /// 
    /// SQL:
    /// SELECT * FROM Users WHERE Id = @id
    /// </summary>
    /// <param name="id">ID của entity</param>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <returns>Entity hoặc null nếu không tìm thấy</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// LẤY TẤT CẢ ENTITIES
    /// 
    /// LƯU Ý:
    /// - KHÔNG nên dùng với bảng lớn (>1000 records)
    /// - Nên dùng GetPaginatedAsync thay thế
    /// 
    /// VÍ DỤ:
    /// var allUsers = await _userRepository.GetAllAsync();
    /// 
    /// SQL:
    /// SELECT * FROM Users WHERE IsDeleted = 0
    /// 
    /// LƯU Ý:
    /// - Query filter tự động loại IsDeleted = true
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// TÌM ENTITIES THEO ĐIỀU KIỆN (predicate)
    /// 
    /// GIẢI THÍCH PREDICATE:
    /// - Expression<Func<T, bool>>: Lambda expression
    /// - Định nghĩa điều kiện filter
    /// - Được convert sang SQL WHERE clause
    /// 
    /// VÍ DỤ:
    /// // Tìm users có email chứa "gmail"
    /// var gmailUsers = await _repository.FindAsync(
    ///     u => u.Email.Contains("gmail.com")
    /// );
    /// 
    /// // Tìm products giá > 1000
    /// var expensiveProducts = await _repository.FindAsync(
    ///     p => p.Price > 1000
    /// );
    /// 
    /// // Tìm orders của user và status = Pending
    /// var pendingOrders = await _repository.FindAsync(
    ///     o => o.UserId == userId && o.Status == OrderStatus.Pending
    /// );
    /// 
    /// SQL:
    /// SELECT * FROM Users WHERE Email LIKE '%gmail.com%'
    /// SELECT * FROM Products WHERE Price > 1000
    /// SELECT * FROM Orders WHERE UserId = @userId AND Status = 1
    /// </summary>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// TÌM ENTITY ĐẦU TIÊN THEO ĐIỀU KIỆN (hoặc null)
    /// 
    /// VÍ DỤ:
    /// // Tìm user theo email
    /// var user = await _repository.FirstOrDefaultAsync(
    ///     u => u.Email == email
    /// );
    /// 
    /// if (user == null)
    ///     return Error.NotFound(...);
    /// 
    /// SQL:
    /// SELECT TOP 1 * FROM Users WHERE Email = @email
    /// 
    /// LƯU Ý:
    /// - Trả về NULL nếu không tìm thấy (không throw exception)
    /// - Tương đương LINQ: query.FirstOrDefault()
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// KIỂM TRA TỒN TẠI (có entity nào thỏa điều kiện không?)
    /// 
    /// VÍ DỤ:
    /// // Kiểm tra email đã tồn tại?
    /// var emailExists = await _repository.AnyAsync(
    ///     u => u.Email == email
    /// );
    /// 
    /// if (emailExists)
    ///     return Error.Conflict(...);
    /// 
    /// // Kiểm tra user có orders không?
    /// var hasOrders = await _orderRepository.AnyAsync(
    ///     o => o.UserId == userId
    /// );
    /// 
    /// SQL:
    /// SELECT CASE WHEN EXISTS (
    ///     SELECT 1 FROM Users WHERE Email = @email
    /// ) THEN 1 ELSE 0 END
    /// 
    /// PERFORMANCE:
    /// - Nhanh hơn Count() > 0
    /// - Dừng ngay khi tìm thấy record đầu tiên
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ĐẾM SỐ ENTITIES
    /// 
    /// VÍ DỤ:
    /// // Đếm tất cả users
    /// var totalUsers = await _repository.CountAsync();
    /// 
    /// // Đếm users active
    /// var activeUsers = await _repository.CountAsync(
    ///     u => u.IsActive == true
    /// );
    /// 
    /// // Đếm orders của user
    /// var userOrderCount = await _orderRepository.CountAsync(
    ///     o => o.UserId == userId
    /// );
    /// 
    /// SQL:
    /// SELECT COUNT(*) FROM Users
    /// SELECT COUNT(*) FROM Users WHERE IsActive = 1
    /// SELECT COUNT(*) FROM Orders WHERE UserId = @userId
    /// </summary>
    /// <param name="predicate">Điều kiện (optional, null = count all)</param>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    // ==================== WRITE OPERATIONS (COMMANDS) ====================

    /// <summary>
    /// THÊM ENTITY MỚI
    /// 
    /// LƯU Ý:
    /// - Chỉ ADD vào context, CHƯA SAVE vào database
    /// - Phải gọi UnitOfWork.SaveChangesAsync() để commit
    /// 
    /// VÍ DỤ:
    /// var user = User.Create(email, username, firstName, lastName);
    /// await _repository.AddAsync(user);
    /// await _unitOfWork.SaveChangesAsync(); // SAVE to DB
    /// 
    /// EF CORE:
    /// _context.Users.Add(user);
    /// // State = Added, chưa có trong DB
    /// 
    /// await _context.SaveChangesAsync();
    /// // INSERT INTO Users... (lúc này mới save)
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// THÊM NHIỀU ENTITIES CÙNG LÚC
    /// 
    /// VÍ DỤ:
    /// var users = new List<User>
    /// {
    ///     User.Create("user1@example.com", ...),
    ///     User.Create("user2@example.com", ...),
    ///     User.Create("user3@example.com", ...)
    /// };
    /// 
    /// await _repository.AddRangeAsync(users);
    /// await _unitOfWork.SaveChangesAsync();
    /// 
    /// PERFORMANCE:
    /// - Nhanh hơn nhiều lần Add() riêng lẻ
    /// - EF Core optimize thành batch insert
    /// 
    /// SQL:
    /// INSERT INTO Users (Id, Email, ...) VALUES
    /// (@id1, @email1, ...),
    /// (@id2, @email2, ...),
    /// (@id3, @email3, ...);
    /// </summary>
    Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CẬP NHẬT ENTITY
    /// 
    /// LƯU Ý:
    /// - Mark entity state = Modified
    /// - CHƯA UPDATE vào database
    /// - Phải gọi SaveChangesAsync() để commit
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.GetByIdAsync(userId);
    /// user.UpdateProfile("New Name", "New LastName");
    /// 
    /// _repository.Update(user); // Mark as modified
    /// await _unitOfWork.SaveChangesAsync(); // UPDATE to DB
    /// 
    /// EF CORE:
    /// _context.Users.Update(user);
    /// // State = Modified
    /// 
    /// await _context.SaveChangesAsync();
    /// // UPDATE Users SET Name = ... WHERE Id = ...
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// CẬP NHẬT NHIỀU ENTITIES
    /// 
    /// VÍ DỤ:
    /// var users = await _repository.FindAsync(u => u.IsActive == false);
    /// foreach (var user in users)
    /// {
    ///     user.Activate();
    /// }
    /// 
    /// _repository.UpdateRange(users);
    /// await _unitOfWork.SaveChangesAsync();
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// XÓA ENTITY
    /// 
    /// LƯU Ý:
    /// - Nếu entity kế thừa BaseAuditableEntity
    /// - Sẽ là SOFT DELETE (IsDeleted = true)
    /// - KHÔNG phải hard delete (DELETE FROM...)
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.GetByIdAsync(userId);
    /// _repository.Remove(user);
    /// await _unitOfWork.SaveChangesAsync();
    /// 
    /// KẾT QUẢ:
    /// // Soft delete
    /// UPDATE Users 
    /// SET IsDeleted = 1, 
    ///     DeletedAt = GETUTCDATE(),
    ///     DeletedBy = @currentUserId
    /// WHERE Id = @userId
    /// 
    /// // KHÔNG phải hard delete:
    /// // DELETE FROM Users WHERE Id = @userId
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// XÓA NHIỀU ENTITIES
    /// 
    /// VÍ DỤ:
    /// var oldUsers = await _repository.FindAsync(
    ///     u => u.CreatedAt < DateTime.UtcNow.AddYears(-5)
    /// );
    /// 
    /// _repository.RemoveRange(oldUsers);
    /// await _unitOfWork.SaveChangesAsync();
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    // ==================== PAGINATION ====================

    /// <summary>
    /// LẤY DANH SÁCH CÓ PHÂN TRANG, SẮP XẾP, TÌM KIẾM
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
    ///     predicate: u => u.IsActive == true
    /// );
    /// 
    /// KẾT QUẢ:
    /// - Items: 10 users của trang 2
    /// - TotalCount: Tổng số users active có "john"
    /// - TotalPages: Tổng số trang
    /// - HasNextPage, HasPreviousPage
    /// 
    /// SQL:
    /// -- Count total
    /// SELECT COUNT(*) FROM Users 
    /// WHERE IsActive = 1 AND (Name LIKE '%john%' OR Email LIKE '%john%')
    /// 
    /// -- Get page data
    /// SELECT * FROM Users
    /// WHERE IsActive = 1 AND (Name LIKE '%john%' OR Email LIKE '%john%')
    /// ORDER BY CreatedAt DESC
    /// OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
    /// </summary>
    Task<PaginatedList<T>> GetPaginatedAsync(
        PaginationRequest request,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}