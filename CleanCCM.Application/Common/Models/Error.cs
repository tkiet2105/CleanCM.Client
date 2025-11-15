using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Common.Models;

/// <summary>
/// CLASS ĐẠI DIỆN CHO MỘT LỖI trong Application Layer
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// ERROR VS EXCEPTION - KHÁC NHAU THẾ NÀO?
/// 
/// EXCEPTION (try-catch):
/// - Dùng cho lỗi BẤT THƯỜNG, không mong đợi
/// - Break flow của program
/// - Tốn performance (expensive)
/// - Ví dụ: Database down, null reference, out of memory
/// 
/// ERROR (Result pattern):
/// - Dùng cho lỗi MONG ĐỢI, có thể xảy ra
/// - KHÔNG break flow
/// - Return như data bình thường
/// - Ví dụ: Email đã tồn tại, sai password, validation fail
/// 
/// KHI NÀO DÙNG ERROR?
/// ✅ Email already exists → Error (business rule)
/// ✅ Invalid password → Error (validation)
/// ✅ Product out of stock → Error (business logic)
/// ❌ Database connection failed → Exception (system error)
/// ❌ Null reference → Exception (programming error)
/// 
/// CÁCH HOẠT ĐỘNG:
/// 
/// CÁCH CŨ (dùng Exception):
/// try {
///     await CreateUserAsync(email);
///     return Ok("Success");
/// }
/// catch (EmailExistsException ex) {
///     return BadRequest(ex.Message); // Lỗi mong đợi dùng exception?
/// }
/// 
/// CÁCH MỚI (dùng Error + Result):
/// var result = await CreateUserAsync(email);
/// if (result.IsFailure)
///     return BadRequest(result.Error); // Rõ ràng, clean hơn
/// return Ok(result.Value);
/// 
/// ƯU ĐIỂM ERROR PATTERN:
/// 1. Rõ ràng: Method signature cho biết có thể fail
/// 2. Performance: Không tốn cost của exception
/// 3. Type-safe: Compiler check
/// 4. Clean code: Dễ đọc, dễ maintain
/// </summary>
public sealed record Error
{
    /// <summary>
    /// MÃ LỖI từ Domain (format: CATEGORY_CODE)
    /// 
    /// VÍ DỤ:
    /// - "AUTH_7001" → Invalid credentials
    /// - "VAL_2001" → Required field
    /// - "NF_1001" → Not found by ID
    /// 
    /// CÁCH DÙNG:
    /// if (result.IsFailure)
    /// {
    ///     switch (result.Error.Code)
    ///     {
    ///         case "AUTH_7001":
    ///             // Handle invalid credentials
    ///             break;
    ///         case "VAL_2001":
    ///             // Handle validation error
    ///             break;
    ///     }
    /// }
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// THÔNG BÁO LỖI bằng ngôn ngữ tự nhiên
    /// 
    /// VÍ DỤ:
    /// - "Email already exists"
    /// - "Invalid email or password"
    /// - "Product not found"
    /// 
    /// LƯU Ý:
    /// - Message từ Resource files (đa ngôn ngữ)
    /// - CÓ THỂ hiển thị trực tiếp cho user
    /// - Nên rõ ràng, dễ hiểu
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// LOẠI LỖI (enum) - để xác định HTTP status code
    /// 
    /// MAPPING VỚI HTTP STATUS:
    /// - Validation → 400 Bad Request
    /// - NotFound → 404 Not Found
    /// - Conflict → 409 Conflict
    /// - Unauthorized → 401 Unauthorized
    /// - Business → 422 Unprocessable Entity
    /// - Failure → 500 Internal Server Error
    /// 
    /// VÍ DỤ:
    /// return error.Type switch
    /// {
    ///     ErrorType.Validation => BadRequest(error),
    ///     ErrorType.NotFound => NotFound(error),
    ///     ErrorType.Unauthorized => Unauthorized(error),
    ///     _ => StatusCode(500, error)
    /// };
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// METADATA BỔ SUNG (optional) - Thông tin thêm về lỗi
    /// 
    /// SỬ DỤNG:
    /// - Validation errors: Chứa field errors
    /// - Business errors: Chứa context data
    /// - Logging: Chứa debug info
    /// 
    /// VÍ DỤ VALIDATION ERRORS:
    /// {
    ///     "fields": {
    ///         "Email": ["Invalid format", "Already exists"],
    ///         "Password": ["Too weak", "Must contain uppercase"]
    ///     }
    /// }
    /// 
    /// VÍ DỤ BUSINESS ERRORS:
    /// {
    ///     "availableStock": 5,
    ///     "requestedQuantity": 10
    /// }
    /// </summary>
    public Dictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Constructor PRIVATE - Chỉ tạo Error qua static methods
    /// 
    /// TẠI SAO PRIVATE?
    /// - Đảm bảo tất cả errors được tạo đúng cách
    /// - Enforce sử dụng ErrorCode từ Domain
    /// - Type-safe, không tạo error tùy tiện
    /// </summary>
    private Error(string code, string message, ErrorType type, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata;
    }

    // ==================== FACTORY METHODS ====================

    /// <summary>
    /// Tạo lỗi CHUNG (Failure/System Error)
    /// 
    /// SỬ DỤNG KHI:
    /// - Lỗi không rõ category
    /// - System error
    /// - Generic failure
    /// 
    /// VÍ DỤ:
    /// return Result.Failure(
    ///     Error.Failure(BaseErrors.UnknownError, "Something went wrong")
    /// );
    /// 
    /// HTTP STATUS: 500 Internal Server Error
    /// </summary>
    public static Error Failure(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.Failure, metadata);

    /// <summary>
    /// Tạo lỗi VALIDATION (dữ liệu không hợp lệ)
    /// 
    /// SỬ DỤNG KHI:
    /// - Email sai format
    /// - Password quá ngắn
    /// - Required field trống
    /// - Dữ liệu đầu vào không đúng
    /// 
    /// VÍ DỤ:
    /// if (string.IsNullOrEmpty(email))
    ///     return Result.Failure(
    ///         Error.Validation(BaseErrors.Required, "Email is required")
    ///     );
    /// 
    /// HTTP STATUS: 400 Bad Request
    /// </summary>
    public static Error Validation(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.Validation, metadata);

    /// <summary>
    /// Tạo lỗi NOT FOUND (không tìm thấy)
    /// 
    /// SỬ DỤNG KHI:
    /// - Tìm User theo ID không có
    /// - Tìm Product không tồn tại
    /// - Resource không tìm thấy
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.GetByIdAsync(userId);
    /// if (user == null)
    ///     return Result.Failure(
    ///         Error.NotFound(BaseErrors.NotFoundById, "User not found")
    ///     );
    /// 
    /// HTTP STATUS: 404 Not Found
    /// </summary>
    public static Error NotFound(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.NotFound, metadata);

    /// <summary>
    /// Tạo lỗi CONFLICT (xung đột dữ liệu)
    /// 
    /// SỬ DỤNG KHI:
    /// - Email đã tồn tại
    /// - Unique constraint violation
    /// - Duplicate key
    /// - Data conflict
    /// 
    /// VÍ DỤ:
    /// var existing = await _repository.FirstOrDefaultAsync(u => u.Email == email);
    /// if (existing != null)
    ///     return Result.Failure(
    ///         Error.Conflict(UserErrors.EmailAlreadyInUse, "Email already exists")
    ///     );
    /// 
    /// HTTP STATUS: 409 Conflict
    /// </summary>
    public static Error Conflict(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.Conflict, metadata);

    /// <summary>
    /// Tạo lỗi UNAUTHORIZED (chưa xác thực hoặc xác thực sai)
    /// 
    /// SỬ DỤNG KHI:
    /// - Login sai password
    /// - Token không hợp lệ
    /// - Token hết hạn
    /// - Chưa đăng nhập
    /// 
    /// VÍ DỤ:
    /// var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
    /// if (!result.Succeeded)
    ///     return Result.Failure(
    ///         Error.Unauthorized(AuthErrors.InvalidCredentials, "Invalid credentials")
    ///     );
    /// 
    /// HTTP STATUS: 401 Unauthorized
    /// </summary>
    public static Error Unauthorized(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.Unauthorized, metadata);

    /// <summary>
    /// Tạo lỗi BUSINESS LOGIC (vi phạm quy tắc nghiệp vụ)
    /// 
    /// SỬ DỤNG KHI:
    /// - Product out of stock
    /// - Cannot cancel shipped order
    /// - Insufficient balance
    /// - Business rule violation
    /// 
    /// VÍ DỤ:
    /// if (product.Stock < quantity)
    ///     return Result.Failure(
    ///         Error.Business(ProductErrors.InsufficientQuantity, "Not enough stock")
    ///     );
    /// 
    /// HTTP STATUS: 422 Unprocessable Entity
    /// </summary>
    public static Error Business(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.Business, metadata);

    /// <summary>
    /// Error NONE - Đại diện cho "không có lỗi"
    /// 
    /// SỬ DỤNG:
    /// - Internal trong Result.Success()
    /// - KHÔNG nên dùng trực tiếp
    /// 
    /// VÍ DỤ:
    /// if (result.Error == Error.None) // Kiểm tra không có lỗi
    ///     Console.WriteLine("Success");
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Tạo lỗi FORBIDDEN - Không có quyền truy cập
    /// 
    /// SỬ DỤNG KHI:
    /// - User role trying to access Admin endpoint
    /// - User trying to modify other user's data
    /// - Insufficient permission/role
    /// 
    /// VÍ DỤ:
    /// if (!User.IsInRole("Admin"))
    ///     return Error.Forbidden(AuthErrors.Forbidden, "Admin permission required");
    /// 
    /// HTTP STATUS: 403 Forbidden
    /// </summary>
    public static Error Forbidden(ErrorCode errorCode, string message, Dictionary<string, object>? metadata = null) =>
        new(errorCode.FullCode, message, ErrorType.Forbidden, metadata);

    // ==================== HELPER METHOD ====================

    /// <summary>
    /// Tạo Validation Error với NHIỀU FIELDS (từ FluentValidation)
    /// 
    /// GIẢI THÍCH:
    /// - FluentValidation trả về Dictionary<string, string[]>
    /// - Key: Tên field (Email, Password...)
    /// - Value: Array of error messages
    /// 
    /// SỬ DỤNG:
    /// - ValidationBehaviour catch FluentValidation errors
    /// - Convert sang Error với metadata chứa field errors
    /// 
    /// VÍ DỤ INPUT:
    /// {
    ///     "Email": ["Invalid format", "Already exists"],
    ///     "Password": ["Too short", "Missing uppercase"]
    /// }
    /// 
    /// VÍ DỤ OUTPUT ERROR:
    /// {
    ///     "code": "VAL_2001",
    ///     "message": "Validation failed",
    ///     "type": "Validation",
    ///     "metadata": {
    ///         "fields": {
    ///             "Email": ["Invalid format", "Already exists"],
    ///             "Password": ["Too short", "Missing uppercase"]
    ///         }
    ///     }
    /// }
    /// 
    /// FRONTEND XỬ LÝ:
    /// if (error.metadata?.fields) {
    ///     Object.keys(error.metadata.fields).forEach(field => {
    ///         showFieldErrors(field, error.metadata.fields[field]);
    ///     });
    /// }
    /// </summary>
    /// <param name="errorCode">Mã lỗi validation (thường là BaseErrors.Required)</param>
    /// <param name="message">Message tổng quát ("Validation failed")</param>
    /// <param name="fieldErrors">Dictionary chứa lỗi của từng field</param>
    /// <returns>Error object với metadata chứa field errors</returns>
    public static Error ValidationWithFields(
        ErrorCode errorCode,
        string message,
        Dictionary<string, string[]> fieldErrors)
    {
        var metadata = new Dictionary<string, object>
        {
            ["fields"] = fieldErrors
        };
        return new Error(errorCode.FullCode, message, ErrorType.Validation, metadata);
    }
}

/// <summary>
/// LOẠI LỖI CHO ERROR HANDLING
/// 
/// ERROR TYPE VS HTTP STATUS CODE:
/// - Failure       → 500 Internal Server Error
/// - Validation    → 400 Bad Request
/// - NotFound      → 404 Not Found
/// - Conflict      → 409 Conflict
/// - Unauthorized  → 401 Unauthorized
/// - Forbidden     → 403 Forbidden
/// - Business      → 422 Unprocessable Entity
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Lỗi chung, system error
    /// HTTP 500
    /// 
    /// VÍ DỤ:
    /// - Database connection error
    /// - Unhandled exception
    /// - Server crash
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Lỗi validation dữ liệu đầu vào
    /// HTTP 400
    /// 
    /// VÍ DỤ:
    /// - Email sai format
    /// - Password quá ngắn
    /// - Required field missing
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Không tìm thấy resource
    /// HTTP 404
    /// 
    /// VÍ DỤ:
    /// - User not found
    /// - Product not found
    /// - Order not found
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Xung đột dữ liệu (duplicate, unique constraint)
    /// HTTP 409
    /// 
    /// VÍ DỤ:
    /// - Email already exists
    /// - Username already taken
    /// - Duplicate entry
    /// </summary>
    Conflict = 3,

    /// <summary>
    /// Chưa xác thực hoặc xác thực sai
    /// HTTP 401
    /// 
    /// VÍ DỤ:
    /// - Invalid credentials
    /// - Token expired
    /// - No authentication header
    /// 
    /// SO SÁNH:
    /// - 401 Unauthorized: Chưa đăng nhập hoặc token sai
    /// - 403 Forbidden: Đã đăng nhập nhưng không đủ quyền
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// Vi phạm business logic
    /// HTTP 422
    /// 
    /// VÍ DỤ:
    /// - Cannot delete user with active orders
    /// - Insufficient balance
    /// - Business rule violation
    /// </summary>
    Business = 5,

    /// <summary>
    /// Không có quyền truy cập (đã authenticated nhưng thiếu permission)
    /// HTTP 403
    /// 
    /// VÍ DỤ:
    /// - User trying to access Admin endpoint
    /// - Manager trying to delete other manager's data
    /// - Insufficient role/permission
    /// 
    /// SO SÁNH:
    /// - 401 Unauthorized: "Bạn là ai?" (Who are you?)
    /// - 403 Forbidden: "Tôi biết bạn là ai, nhưng bạn không được phép" (You can't do this)
    /// 
    /// VÍ DỤ CỤ THỂ:
    /// 
    /// CASE 1 - 401 Unauthorized:
    /// GET /api/users/me
    /// (Không có token)
    /// → 401: "Please login first"
    /// 
    /// CASE 2 - 403 Forbidden:
    /// DELETE /api/users/{id}
    /// Authorization: Bearer {user_token}  ← User role
    /// → 403: "You don't have permission (Admin only)"
    /// </summary>
    Forbidden = 6
}