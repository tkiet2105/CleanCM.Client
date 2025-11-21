namespace CleanCCM.Application.Common.Interfaces;

/// <summary>
/// SERVICE LẤY THÔNG TIN USER HIỆN TẠI (đang đăng nhập)
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// CURRENT USER LÀ GÌ?
/// - User đang thực hiện request
/// - User đã login và có JWT token
/// - Thông tin lấy từ HttpContext.User (Claims)
/// 
/// TẠI SAO CẦN SERVICE NÀY?
/// 
/// 1. AUDIT TRAIL:
///    - Ai tạo? → CreatedBy = currentUserId
///    - Ai sửa? → LastModifiedBy = currentUserId
///    - Ai xóa? → DeletedBy = currentUserId
/// 
/// 2. AUTHORIZATION:
///    - Chỉ owner mới được edit
///    - Check permissions dựa trên userId
/// 
/// 3. BUSINESS LOGIC:
///    - Lấy orders của user hiện tại
///    - User không thể xóa chính mình
///    - Filter dữ liệu theo user
/// 
/// CÁCH HOẠT ĐỘNG:
/// 
/// 1. USER LOGIN:
///    POST /api/auth/login
///    { "email": "user@example.com", "password": "..." }
///    
///    ↓ Server tạo JWT token với Claims:
///    {
///      "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///      "name": "john_doe",
///      "email": "user@example.com",
///      "role": "User"
///    }
/// 
/// 2. CLIENT LƯU TOKEN:
///    localStorage.setItem('token', accessToken)
/// 
/// 3. SUBSEQUENT REQUESTS:
///    GET /api/users/me
///    Headers: {
///      "Authorization": "Bearer eyJhbGc..."
///    }
///    
///    ↓ Middleware decode JWT
///    ↓ Set HttpContext.User với Claims
///    ↓ CurrentUserService đọc từ HttpContext.User
/// 
/// VÍ DỤ SỬ DỤNG:
/// 
/// // Trong Handler
/// public class CreateOrderCommandHandler
/// {
///     private readonly ICurrentUserService _currentUserService;
///     
///     public async Task<Result> Handle(CreateOrderCommand request)
///     {
///         // Lấy userId của user đang login
///         var userId = _currentUserService.UserId;
///         
///         var order = Order.Create(
///             userId: userId,  // Order thuộc user này
///             items: request.Items
///         );
///         
///         await _orderRepository.AddAsync(order);
///         await _unitOfWork.SaveChangesAsync();
///         
///         return Result.Success();
///     }
/// }
/// 
/// // Trong Interceptor (Audit)
/// public class AuditableEntityInterceptor
/// {
///     public void UpdateEntities(DbContext context)
///     {
///         var userId = _currentUserService.UserId ?? "System";
///         
///         foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
///         {
///             if (entry.State == EntityState.Added)
///             {
///                 entry.Entity.CreatedBy = userId;
///             }
///             
///             if (entry.State == EntityState.Modified)
///             {
///                 entry.Entity.LastModifiedBy = userId;
///             }
///         }
///     }
/// }
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }

    bool IsAuthenticated { get; }

    /// <summary>
    /// Danh sách các role mà user đang đăng nhập sở hữu
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Kiểm tra user có role cụ thể không
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Kiểm tra user có thuộc bất kỳ role nào trong danh sách không
    /// </summary>
    bool IsInRoles(params string[] roles);

    /// <summary>
    /// Check nhanh xem user có phải Administrator không
    /// </summary>
    bool IsAdmin { get; }
}