namespace CleanCCM.Domain.Common.Errors;

/// <summary>
/// DANH SÁCH CÁC LỖI CHUNG cho toàn hệ thống
/// 
/// QUAN TRỌNG:
/// - File này chứa các lỗi PHỔ BIẾN được sử dụng cho MỌI entity
/// - Khi tạo entity mới (User, Product, Order...), ưu tiên dùng lỗi từ đây
/// - Chỉ tạo lỗi riêng khi có nghiệp vụ ĐẶC THÙ
/// 
/// QUY TẮC MÃ LỖI:
/// - Not Found:     1000 - 1999
/// - Validation:    2000 - 2999
/// - Conflict:      3000 - 3999
/// - Business:      4000 - 4999
/// - System:        5000 - 5999
/// - Authorization: 6000 - 6999
/// 
/// LƯU Ý:
/// - KHÔNG được dùng mã < 1000 (để tránh trùng với các hệ thống khác)
/// - Mỗi nhóm có 1000 mã → đủ cho mở rộng
/// </summary>
public static class BaseErrors
{
    // ==================== NOT FOUND ERRORS (1000-1999) ====================

    /// <summary>
    /// Entity không tìm thấy theo ID
    /// 
    /// SỬ DỤNG KHI:
    /// - Tìm User theo ID không có
    /// - Tìm Product theo ID không có
    /// - Tìm bất kỳ entity nào theo ID không có
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.GetByIdAsync(userId);
    /// if (user == null)
    ///     return Result.Failure(Error.NotFound(BaseErrors.NotFoundById, "User not found"));
    /// </summary>
    public static readonly ErrorCode NotFoundById =
        ErrorCode.Create(ErrorCategory.NotFound, 1001);

    /// <summary>
    /// Không tìm thấy entity theo điều kiện lọc/tìm kiếm
    /// 
    /// SỬ DỤNG KHI:
    /// - Tìm kiếm theo tên, email, phone... không có kết quả
    /// - Filter theo nhiều điều kiện không có dữ liệu
    /// 
    /// VÍ DỤ:
    /// var users = await _repository.FindAsync(u => u.Email == email);
    /// if (!users.Any())
    ///     return Result.Failure(Error.NotFound(BaseErrors.NotFoundByFilter, "No users match"));
    /// </summary>
    public static readonly ErrorCode NotFoundByFilter =
        ErrorCode.Create(ErrorCategory.NotFound, 1002);

    /// <summary>
    /// Danh sách/Collection trống
    /// 
    /// SỬ DỤNG KHI:
    /// - Lấy danh sách nhưng không có item nào
    /// - Query trả về empty list
    /// </summary>
    public static readonly ErrorCode EmptyList =
        ErrorCode.Create(ErrorCategory.NotFound, 1003);



    // ==================== VALIDATION ERRORS (2000-2999) ====================

    /// <summary>
    /// Trường bắt buộc (required) bị bỏ trống
    /// 
    /// SỬ DỤNG KHI:
    /// - Email, Name, Password... không được nhập
    /// - Trường mandatory không có giá trị
    /// 
    /// VÍ DỤ FluentValidation:
    /// RuleFor(x => x.Email)
    ///     .NotEmpty().WithErrorCode(BaseErrors.Required.FullCode);
    /// </summary>
    public static readonly ErrorCode Required =
        ErrorCode.Create(ErrorCategory.Validation, 2001);

    /// <summary>
    /// Định dạng dữ liệu không đúng
    /// 
    /// SỬ DỤNG KHI:
    /// - Email không đúng format (abc@xyz.com)
    /// - Phone không đúng format (0901234567)
    /// - Date không parse được
    /// </summary>
    public static readonly ErrorCode InvalidFormat =
        ErrorCode.Create(ErrorCategory.Validation, 2002);

    /// <summary>
    /// Độ dài chuỗi không hợp lệ (quá ngắn hoặc quá dài)
    /// 
    /// SỬ DỤNG KHI:
    /// - Password < 8 ký tự
    /// - Name > 100 ký tự
    /// - Description vượt quá max length
    /// 
    /// VÍ DỤ:
    /// RuleFor(x => x.Password)
    ///     .MinimumLength(8).WithErrorCode(BaseErrors.InvalidLength.FullCode);
    /// </summary>
    public static readonly ErrorCode InvalidLength =
        ErrorCode.Create(ErrorCategory.Validation, 2003);

    /// <summary>
    /// Giá trị nằm ngoài phạm vi cho phép
    /// 
    /// SỬ DỤNG KHI:
    /// - Age < 0 hoặc > 150
    /// - Quantity < 0
    /// - Price ngoài khoảng min/max
    /// 
    /// VÍ DỤ:
    /// if (request.Age < 0 || request.Age > 150)
    ///     return Error.Validation(BaseErrors.OutOfRange, "Age must be 0-150");
    /// </summary>
    public static readonly ErrorCode OutOfRange =
        ErrorCode.Create(ErrorCategory.Validation, 2004);

    /// <summary>
    /// Email không hợp lệ (format sai)
    /// 
    /// SỬ DỤNG KHI:
    /// - Email không có @
    /// - Email không đúng pattern
    /// </summary>
    public static readonly ErrorCode InvalidEmail =
        ErrorCode.Create(ErrorCategory.Validation, 2005);

    /// <summary>
    /// Số điện thoại không hợp lệ
    /// 
    /// SỬ DỤNG KHI:
    /// - Phone không đủ số
    /// - Phone có ký tự đặc biệt không cho phép
    /// </summary>
    public static readonly ErrorCode InvalidPhone =
        ErrorCode.Create(ErrorCategory.Validation, 2006);

    /// <summary>
    /// URL không hợp lệ
    /// 
    /// SỬ DỤNG KHI:
    /// - Website URL sai format
    /// - Image URL không parse được
    /// </summary>
    public static readonly ErrorCode InvalidUrl =
        ErrorCode.Create(ErrorCategory.Validation, 2007);

    /// <summary>
    /// Ngày tháng không hợp lệ
    /// 
    /// SỬ DỤNG KHI:
    /// - Date không parse được
    /// - Date trong quá khứ khi yêu cầu future
    /// - Date vượt quá hiện tại khi yêu cầu past
    /// </summary>
    public static readonly ErrorCode InvalidDate =
        ErrorCode.Create(ErrorCategory.Validation, 2008);

    /// <summary>
    /// Giá trị phải > 0
    /// 
    /// SỬ DỤNG KHI:
    /// - Price <= 0
    /// - Quantity <= 0
    /// - Amount <= 0
    /// </summary>
    public static readonly ErrorCode MustBePositive =
        ErrorCode.Create(ErrorCategory.Validation, 2009);

    /// <summary>
    /// Giá trị phải >= 0 (cho phép bằng 0)
    /// 
    /// SỬ DỤNG KHI:
    /// - Discount có thể = 0
    /// - Stock quantity có thể = 0
    /// </summary>
    public static readonly ErrorCode MustBeNonNegative =
        ErrorCode.Create(ErrorCategory.Validation, 2010);
    public static readonly ErrorCode InvalidValue =
            ErrorCode.Create(ErrorCategory.Validation, 2011);
    public static readonly ErrorCode InvalidId =
        ErrorCode.Create(ErrorCategory.Validation, 2012);
    // ==================== CONFLICT ERRORS (3000-3999) ====================

    /// <summary>
    /// Entity đã tồn tại (duplicate - trùng lặp)
    /// 
    /// SỬ DỤNG KHI:
    /// - Tạo entity với ID/key đã có
    /// - Insert duplicate record
    /// 
    /// VÍ DỤ:
    /// var existing = await _repository.FirstOrDefaultAsync(x => x.Email == email);
    /// if (existing != null)
    ///     return Error.Conflict(BaseErrors.AlreadyExists, "Email exists");
    /// </summary>
    public static readonly ErrorCode AlreadyExists =
        ErrorCode.Create(ErrorCategory.Conflict, 3001);

    /// <summary>
    /// Vi phạm ràng buộc unique key/constraint trong database
    /// 
    /// SỬ DỤNG KHI:
    /// - Catch SqlException với unique constraint violation
    /// - Insert/Update vi phạm unique index
    /// </summary>
    public static readonly ErrorCode DuplicateKey =
        ErrorCode.Create(ErrorCategory.Conflict, 3002);

    /// <summary>
    /// Không thể xóa vì có tham chiếu (foreign key)
    /// 
    /// SỬ DỤNG KHI:
    /// - Xóa Category nhưng còn Product tham chiếu
    /// - Xóa User nhưng còn Order của user đó
    /// 
    /// VÍ DỤ:
    /// var hasOrders = await _orderRepository.AnyAsync(o => o.UserId == userId);
    /// if (hasOrders)
    ///     return Error.Conflict(BaseErrors.CannotDeleteHasReference, "User has orders");
    /// </summary>
    public static readonly ErrorCode CannotDeleteHasReference =
        ErrorCode.Create(ErrorCategory.Conflict, 3003);

    /// <summary>
    /// Entity đang được sử dụng, không thể thao tác
    /// 
    /// SỬ DỤNG KHI:
    /// - Product đang trong giỏ hàng
    /// - User đang online
    /// - Resource đang bị lock
    /// </summary>
    public static readonly ErrorCode InUse =
        ErrorCode.Create(ErrorCategory.Conflict, 3004);

    // ==================== BUSINESS ERRORS (4000-4999) ====================

    /// <summary>
    /// Thao tác TẠO MỚI thất bại
    /// 
    /// SỬ DỤNG KHI:
    /// - Create User/Product/Order... không thành công
    /// - Insert vào database fail
    /// 
    /// VÍ DỤ:
    /// var result = await _userManager.CreateAsync(user, password);
    /// if (!result.Succeeded)
    ///     return Error.Business(BaseErrors.CreationFailed, "Create user failed");
    /// </summary>
    public static readonly ErrorCode CreationFailed =
        ErrorCode.Create(ErrorCategory.Business, 4001);

    /// <summary>
    /// Thao tác CẬP NHẬT thất bại
    /// 
    /// SỬ DỤNG KHI:
    /// - Update entity không thành công
    /// - SaveChanges fail sau Update
    /// </summary>
    public static readonly ErrorCode UpdateFailed =
        ErrorCode.Create(ErrorCategory.Business, 4002);

    /// <summary>
    /// Thao tác XÓA thất bại
    /// 
    /// SỬ DỤNG KHI:
    /// - Delete entity không thành công
    /// - SaveChanges fail sau Remove
    /// </summary>
    public static readonly ErrorCode DeletionFailed =
        ErrorCode.Create(ErrorCategory.Business, 4003);

    /// <summary>
    /// Trạng thái không hợp lệ cho thao tác này
    /// 
    /// SỬ DỤNG KHI:
    /// - Cancel order đã shipped
    /// - Edit published article
    /// - Reactive deleted account
    /// 
    /// VÍ DỤ:
    /// if (order.Status == OrderStatus.Shipped)
    ///     return Error.Business(BaseErrors.InvalidState, "Cannot cancel shipped order");
    /// </summary>
    public static readonly ErrorCode InvalidState =
        ErrorCode.Create(ErrorCategory.Business, 4004);

    /// <summary>
    /// Không có quyền thực hiện thao tác này (về mặt business)
    /// 
    /// SỬ DỤNG KHI:
    /// - User thường không thể xóa User khác
    /// - Không phải owner không thể edit
    /// 
    /// CHÚ Ý: Khác với Authorization (6xxx) - đây là business rule
    /// </summary>
    public static readonly ErrorCode OperationNotAllowed =
        ErrorCode.Create(ErrorCategory.Business, 4005);

    // ==================== SYSTEM ERRORS (5000-5999) ====================

    /// <summary>
    /// Lỗi database (connection, query, transaction...)
    /// 
    /// SỬ DỤNG KHI:
    /// - Catch SqlException
    /// - Database timeout
    /// - Transaction rollback
    /// 
    /// VÍ DỤ:
    /// try { await _context.SaveChangesAsync(); }
    /// catch (DbUpdateException ex)
    /// {
    ///     return Error.Failure(BaseErrors.DatabaseError, "DB error");
    /// }
    /// </summary>
    public static readonly ErrorCode DatabaseError =
        ErrorCode.Create(ErrorCategory.System, 5001);

    /// <summary>
    /// Lỗi kết nối (network, service unavailable...)
    /// 
    /// SỬ DỤNG KHI:
    /// - Không connect được database
    /// - API external không phản hồi
    /// - Network timeout
    /// </summary>
    public static readonly ErrorCode ConnectionError =
        ErrorCode.Create(ErrorCategory.System, 5002);

    /// <summary>
    /// Lỗi không xác định / Unhandled exception
    /// 
    /// SỬ DỤNG KHI:
    /// - Catch exception chung
    /// - Lỗi không rõ nguyên nhân
    /// </summary>
    public static readonly ErrorCode UnknownError =
        ErrorCode.Create(ErrorCategory.System, 5003);

    /// <summary>
    /// Timeout - Hết thời gian chờ
    /// 
    /// SỬ DỤNG KHI:
    /// - Request quá lâu không có response
    /// - Operation timeout
    /// </summary>
    public static readonly ErrorCode TimeoutError =
        ErrorCode.Create(ErrorCategory.System, 5004);

    // ==================== AUTHORIZATION ERRORS (6000-6999) ====================

    /// <summary>
    /// Forbidden - Không có quyền truy cập (đã authenticated nhưng không đủ quyền)
    /// 
    /// SỬ DỤNG KHI:
    /// - User đã đăng nhập nhưng không có role phù hợp
    /// - Truy cập resource không thuộc quyền sở hữu
    /// 
    /// VÍ DỤ:
    /// if (!User.IsInRole("Admin"))
    ///     return Error.Failure(BaseErrors.Forbidden, "Admin only");
    /// </summary>
    public static readonly ErrorCode Forbidden =
        ErrorCode.Create(ErrorCategory.Authorization, 6001);

    /// <summary>
    /// Unauthorized - Chưa xác thực (chưa đăng nhập)
    /// 
    /// SỬ DỤNG KHI:
    /// - Truy cập endpoint cần đăng nhập nhưng chưa có token
    /// - Token không hợp lệ/thiếu
    /// </summary>
    public static readonly ErrorCode Unauthorized =
        ErrorCode.Create(ErrorCategory.Authorization, 6002);

    /// <summary>
    /// Token không hợp lệ (JWT malformed, signature sai...)
    /// 
    /// SỬ DỤNG KHI:
    /// - JWT không parse được
    /// - Signature không match
    /// - Token bị tamper
    /// </summary>
    public static readonly ErrorCode InvalidToken =
        ErrorCode.Create(ErrorCategory.Authorization, 6003);

    /// <summary>
    /// Token đã hết hạn
    /// 
    /// SỬ DỤNG KHI:
    /// - JWT exp claim < current time
    /// - Refresh token expired
    /// </summary>
    public static readonly ErrorCode ExpiredToken =
        ErrorCode.Create(ErrorCategory.Authorization, 6004);
}