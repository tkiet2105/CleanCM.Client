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
    /// <summary>
    /// USER ID của user đang đăng nhập
    /// 
    /// GIẢI THÍCH:
    /// - Lấy từ JWT claim "nameid" (NameIdentifier)
    /// - null nếu user chưa login
    /// - null nếu request không có Authorization header
    /// 
    /// VÍ DỤ:
    /// var userId = _currentUserService.UserId;
    /// if (userId == null)
    ///     return Error.Unauthorized(..., "Please login");
    /// 
    /// var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
    /// 
    /// JWT CLAIM:
    /// new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    /// → UserId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// 
    /// LƯU Ý:
    /// - Type là string (vì claim là string)
    /// - Cần parse sang Guid nếu dùng làm ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// USERNAME của user đang đăng nhập
    /// 
    /// GIẢI THÍCH:
    /// - Lấy từ JWT claim "unique_name" (Name)
    /// - null nếu chưa login
    /// 
    /// VÍ DỤ:
    /// var userName = _currentUserService.UserName;
    /// Console.WriteLine($"Action performed by: {userName}");
    /// 
    /// JWT CLAIM:
    /// new Claim(ClaimTypes.Name, user.UserName)
    /// → UserName = "john_doe"
    /// 
    /// SỬ DỤNG:
    /// - Hiển thị trong logs
    /// - Audit trail
    /// - Welcome message
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// EMAIL của user đang đăng nhập
    /// 
    /// GIẢI THÍCH:
    /// - Lấy từ JWT claim "email"
    /// - null nếu chưa login
    /// 
    /// VÍ DỤ:
    /// var email = _currentUserService.Email;
    /// await _emailService.SendNotificationAsync(email, "Order created");
    /// 
    /// JWT CLAIM:
    /// new Claim(ClaimTypes.Email, user.Email)
    /// → Email = "user@example.com"
    /// 
    /// SỬ DỤNG:
    /// - Gửi email notification
    /// - Hiển thị trong UI
    /// - Logging
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// USER ĐÃ ĐĂNG NHẬP CHƯA?
    /// 
    /// GIẢI THÍCH:
    /// - true: User đã login (có valid token)
    /// - false: Anonymous user (chưa login)
    /// 
    /// VÍ DỤ:
    /// if (!_currentUserService.IsAuthenticated)
    ///     return Error.Unauthorized(..., "Authentication required");
    /// 
    /// // User đã login, tiếp tục xử lý
    /// var userId = _currentUserService.UserId;
    /// 
    /// IMPLEMENTATION:
    /// IsAuthenticated = HttpContext.User?.Identity?.IsAuthenticated ?? false
    /// 
    /// KHI NÀO TRUE?
    /// - Request có Authorization header
    /// - JWT token hợp lệ
    /// - Token chưa expire
    /// - Signature đúng
    /// 
    /// KHI NÀO FALSE?
    /// - Không có Authorization header
    /// - Token invalid/expired
    /// - Anonymous request
    /// 
    /// USE CASES:
    /// 
    /// // Check authentication
    /// if (!_currentUserService.IsAuthenticated)
    /// {
    ///     return Result.Failure(
    ///         Error.Unauthorized(BaseErrors.Unauthorized, "Please login")
    ///     );
    /// }
    /// 
    /// // Optional authentication
    /// var userId = _currentUserService.IsAuthenticated
    ///     ? _currentUserService.UserId
    ///     : null;
    /// 
    /// // Public endpoint nhưng có personalization cho logged-in users
    /// var products = await GetProductsAsync();
    /// if (_currentUserService.IsAuthenticated)
    /// {
    ///     // Add personalized recommendations
    ///     products = AddRecommendations(products, _currentUserService.UserId);
    /// }
    /// </summary>
    bool IsAuthenticated { get; }
}