using Microsoft.AspNetCore.Identity;

namespace CleanCCM.Infrastructure.Identity;

/// <summary>
/// APPLICATION USER - Entity cho user authentication
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// APPLICATION USER LÀ GÌ?
/// - Kế thừa IdentityUser từ ASP.NET Identity
/// - Entity đại diện cho user trong hệ thống
/// - Lưu trong bảng AspNetUsers
/// 
/// IdentityUser CÓ SẴN GÌ?
/// - Id (string): Primary key
/// - UserName: Tên đăng nhập
/// - Email: Email
/// - PasswordHash: Password đã hash
/// - PhoneNumber: Số điện thoại
/// - EmailConfirmed: Email đã verify chưa
/// - LockoutEnabled: Có thể lock account không
/// - AccessFailedCount: Số lần login fail
/// - ... và nhiều fields khác
/// 
/// TẠI SAO KẾ THỪA?
/// - Tận dụng sẵn ASP.NET Identity
/// - Password hashing, email confirmation, 2FA...
/// - THÊM custom fields cho business
/// 
/// VÍ DỤ:
/// var user = new ApplicationUser
/// {
///     UserName = "john_doe",
///     Email = "john@example.com",
///     FirstName = "John",        // Custom field
///     LastName = "Doe",          // Custom field
///     RefreshToken = "xyz123"    // Custom field
/// };
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// TÊN của user
    /// 
    /// VÍ DỤ:
    /// FirstName = "John"
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// HỌ của user
    /// 
    /// VÍ DỤ:
    /// LastName = "Doe"
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// REFRESH TOKEN hiện tại của user
    /// 
    /// GIẢI THÍCH:
    /// - Token để lấy access token mới
    /// - Lưu trong database
    /// - Expire sau 7 ngày (configurable)
    /// - null nếu chưa login hoặc đã revoke
    /// 
    /// FLOW:
    /// 1. Login → Generate refresh token → Lưu vào RefreshToken
    /// 2. Access token expire → Dùng refresh token
    /// 3. Refresh → Generate new tokens → Update RefreshToken
    /// 4. Logout → Set RefreshToken = null (revoke)
    /// 
    /// BẢO MẬT:
    /// - Mỗi lần refresh → token mới
    /// - Token cũ bị invalidate
    /// - Có thể revoke (logout all devices)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// THỜI GIAN HẾT HẠN của refresh token
    /// 
    /// GIẢI THÍCH:
    /// - DateTime khi refresh token expire
    /// - null nếu không có refresh token
    /// - Check khi validate refresh token
    /// 
    /// VÍ DỤ:
    /// RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    /// 
    /// if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
    /// {
    ///     // Token đã expire
    ///     return Error.Unauthorized(...);
    /// }
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }

    /// <summary>
    /// NGÀY TẠO user
    /// 
    /// VÍ DỤ:
    /// CreatedAt = DateTime.UtcNow;  // "2024-01-15T10:30:00Z"
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// USER CÓ ĐANG HOẠT ĐỘNG KHÔNG?
    /// 
    /// GIẢI THÍCH:
    /// - true: User bình thường
    /// - false: User bị deactivate/suspend
    /// 
    /// KHI NÀO false?
    /// - Admin deactivate user
    /// - User vi phạm policy
    /// - User tự deactivate account
    /// 
    /// CHECK KHI LOGIN:
    /// if (!user.IsActive)
    ///     return Error.Unauthorized(AuthErrors.AccountInactive, ...);
    /// 
    /// VÍ DỤ:
    /// IsActive = true   // Bình thường
    /// IsActive = false  // Bị vô hiệu hóa
    /// </summary>
    public bool IsActive { get; set; } = true;
}