using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Domain.Errors;

/// <summary>
/// CÁC LỖI ĐẶC THÙ CHO USER ENTITY
/// 
/// QUAN TRỌNG:
/// - File này CHỈ chứa lỗi RIÊNG của User
/// - Các lỗi CHUNG đã có trong BaseErrors:
///   + BaseErrors.NotFoundById       → User not found
///   + BaseErrors.CreationFailed     → Create user failed
///   + BaseErrors.UpdateFailed       → Update user failed
///   + BaseErrors.AlreadyExists      → User already exists (general)
/// 
/// QUY TẮC MÃ LỖI:
/// - Conflict:  8000 - 8099
/// - Business:  8100 - 8199
/// - Validation: 8200 - 8299
/// 
/// VÍ DỤ SỬ DỤNG:
/// - User not found by ID        → Dùng BaseErrors.NotFoundById
/// - Email already in use        → Dùng UserErrors.EmailAlreadyInUse
/// - Cannot delete self          → Dùng UserErrors.CannotDeleteSelf
/// </summary>
public static class UserErrors
{
    // ==================== USER CONFLICT ERRORS (8000-8099) ====================

    /// <summary>
    /// Email đã được sử dụng bởi user khác
    /// 
    /// SỬ DỤNG KHI:
    /// - Register với email đã tồn tại
    /// - Update user với email của người khác
    /// 
    /// VÍ DỤ:
    /// var existingUser = await _userManager.FindByEmailAsync(email);
    /// if (existingUser != null)
    ///     return Error.Conflict(UserErrors.EmailAlreadyInUse, "Email already in use");
    /// 
    /// SO SÁNH:
    /// - BaseErrors.AlreadyExists → Dùng cho "User already exists" (chung chung)
    /// - UserErrors.EmailAlreadyInUse → Cụ thể là EMAIL bị trùng
    /// </summary>
    public static readonly ErrorCode EmailAlreadyInUse =
        ErrorCode.Create(ErrorCategory.Conflict, 8001);

    /// <summary>
    /// Username đã được sử dụng
    /// 
    /// SỬ DỤNG KHI:
    /// - Register với username đã tồn tại
    /// - Update username bị trùng
    /// 
    /// VÍ DỤ:
    /// var existingUser = await _userManager.FindByNameAsync(userName);
    /// if (existingUser != null)
    ///     return Error.Conflict(UserErrors.UsernameAlreadyInUse, "Username taken");
    /// </summary>
    public static readonly ErrorCode UsernameAlreadyInUse =
        ErrorCode.Create(ErrorCategory.Conflict, 8002);

    /// <summary>
    /// Số điện thoại đã được sử dụng
    /// 
    /// SỬ DỤNG KHI:
    /// - Phone number unique constraint violation
    /// - Update phone bị trùng
    /// 
    /// VÍ DỤ:
    /// var existingPhone = await _repository.FirstOrDefaultAsync(u => u.Phone == phone);
    /// if (existingPhone != null)
    ///     return Error.Conflict(UserErrors.PhoneAlreadyInUse, "Phone in use");
    /// </summary>
    public static readonly ErrorCode PhoneAlreadyInUse =
        ErrorCode.Create(ErrorCategory.Conflict, 8003);

    // ==================== USER BUSINESS RULES (8100-8199) ====================

    /// <summary>
    /// Không thể tự xóa chính mình
    /// 
    /// SỬ DỤNG KHI:
    /// - Admin/User cố xóa account của chính mình
    /// - userId == currentUserId
    /// 
    /// VÍ DỤ:
    /// if (userId == _currentUserService.UserId)
    ///     return Error.Business(UserErrors.CannotDeleteSelf, "Cannot delete yourself");
    /// 
    /// LÝ DO: Ngăn admin vô tình xóa chính mình và mất quyền quản trị
    /// </summary>
    public static readonly ErrorCode CannotDeleteSelf =
        ErrorCode.Create(ErrorCategory.Business, 8101);

    /// <summary>
    /// Không thể thay đổi role của chính mình
    /// 
    /// SỬ DỤNG KHI:
    /// - Admin cố remove Admin role của chính mình
    /// - User thay đổi role của chính mình
    /// 
    /// VÍ DỤ:
    /// if (userId == _currentUserService.UserId)
    ///     return Error.Business(UserErrors.CannotChangeOwnRole, "Cannot change your own role");
    /// 
    /// LÝ DO: Tránh admin vô tình remove quyền admin của mình
    /// </summary>
    public static readonly ErrorCode CannotChangeOwnRole =
        ErrorCode.Create(ErrorCategory.Business, 8102);

    /// <summary>
    /// Không thể vô hiệu hóa (deactivate) chính mình
    /// 
    /// SỬ DỤNG KHI:
    /// - Admin cố set IsActive = false cho chính mình
    /// 
    /// VÍ DỤ:
    /// if (userId == _currentUserService.UserId && !request.IsActive)
    ///     return Error.Business(UserErrors.CannotDeactivateSelf, "Cannot deactivate yourself");
    /// 
    /// LÝ DO: Tránh admin vô tình khóa tài khoản của mình
    /// </summary>
    public static readonly ErrorCode CannotDeactivateSelf =
        ErrorCode.Create(ErrorCategory.Business, 8103);

    /// <summary>
    /// User phải có ít nhất 1 role
    /// 
    /// SỬ DỤNG KHI:
    /// - Remove tất cả roles của user
    /// - Tạo user không assign role nào
    /// 
    /// VÍ DỤ:
    /// var userRoles = await _userManager.GetRolesAsync(user);
    /// if (!userRoles.Any())
    ///     return Error.Business(UserErrors.MustHaveAtLeastOneRole, "User must have at least one role");
    /// 
    /// LÝ DO: Business rule - mọi user phải có role để phân quyền
    /// </summary>
    public static readonly ErrorCode MustHaveAtLeastOneRole =
        ErrorCode.Create(ErrorCategory.Business, 8104);

    // ==================== USER VALIDATION (8200-8299) ====================

    /// <summary>
    /// Avatar không hợp lệ (file size, format, dimensions...)
    /// 
    /// SỬ DỤNG KHI:
    /// - Upload avatar > max size (vd: 5MB)
    /// - Avatar không phải image (jpg, png...)
    /// - Avatar dimensions quá lớn (vd: > 2000x2000)
    /// 
    /// VÍ DỤ:
    /// if (avatarFile.Length > 5 * 1024 * 1024) // 5MB
    ///     return Error.Validation(UserErrors.InvalidAvatar, "Avatar too large");
    /// 
    /// if (!allowedTypes.Contains(avatarFile.ContentType))
    ///     return Error.Validation(UserErrors.InvalidAvatar, "Invalid avatar format");
    /// </summary>
    public static readonly ErrorCode InvalidAvatar =
        ErrorCode.Create(ErrorCategory.Validation, 8201);

    /// <summary>
    /// Profile không đầy đủ thông tin bắt buộc
    /// 
    /// SỬ DỤNG KHI:
    /// - Yêu cầu complete profile trước khi thực hiện action
    /// - Thiếu FirstName, LastName, Phone... (tùy business)
    /// 
    /// VÍ DỤ:
    /// if (string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName))
    ///     return Error.Validation(UserErrors.IncompleteProfile, "Please complete your profile");
    /// 
    /// SỬ DỤNG: Khi business yêu cầu profile đầy đủ mới được mua hàng, đặt order...
    /// </summary>
    public static readonly ErrorCode IncompleteProfile =
        ErrorCode.Create(ErrorCategory.Validation, 8202);
}