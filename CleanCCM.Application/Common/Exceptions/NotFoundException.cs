namespace CleanCCM.Application.Common.Exceptions;

/// <summary>
/// EXCEPTION CHO LỖI KHÔNG TÌM THẤY (Not Found)
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// NOT FOUND EXCEPTION LÀ GÌ?
/// - Exception được throw khi TÌM KIẾM entity KHÔNG CÓ
/// - Thường dùng khi query theo ID
/// - HTTP 404 Not Found
/// 
/// KHI NÀO DÙNG?
/// - GetById nhưng entity không tồn tại
/// - Update/Delete entity không có
/// - Resource không tìm thấy
/// 
/// VÍ DỤ THỰC TẾ:
/// 
/// 1. USER REQUEST:
///    GET /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
/// 
/// 2. HANDLER QUERY:
///    var user = await _repository.GetByIdAsync(userId);
///    if (user == null)
///        throw new NotFoundException("User", userId);
/// 
/// 3. EXCEPTION MIDDLEWARE BẮT:
///    catch (NotFoundException ex)
///    {
///        return NotFound(new { message = ex.Message });
///    }
/// 
/// 4. RESPONSE:
///    Status: 404 Not Found
///    {
///      "message": "Entity \"User\" (3fa85f64-5717-4562-b3fc-2c963f66afa6) was not found."
///    }
/// 
/// SO SÁNH CÁCH XỬ LÝ:
/// 
/// CÁCH 1 - Dùng Exception (Imperative):
/// var user = await _repository.GetByIdAsync(userId);
/// if (user == null)
///     throw new NotFoundException("User", userId);
/// 
/// // Continue with user
/// user.UpdateProfile(...);
/// 
/// CÁCH 2 - Dùng Result Pattern (Functional):
/// var user = await _repository.GetByIdAsync(userId);
/// if (user == null)
///     return Result.Failure(Error.NotFound(BaseErrors.NotFoundById, "User not found"));
/// 
/// // Continue with user
/// user.UpdateProfile(...);
/// return Result.Success();
/// 
/// KHI NÀO DÙNG EXCEPTION vs RESULT?
/// 
/// DÙNG EXCEPTION KHI:
/// - Legacy code
/// - Quick prototyping
/// - Exceptional case (thực sự bất thường)
/// 
/// DÙNG RESULT KHI:
/// - Clean architecture
/// - Expected failure (lỗi mong đợi)
/// - Functional programming style
/// - Better type safety
/// 
/// LƯU Ý:
/// - Project này ưu tiên Result Pattern
/// - NotFoundException để tương thích legacy code
/// - Nên migrate sang Result dần dần
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Constructor MẶC ĐỊNH - Message rỗng
    /// 
    /// SỬ DỤNG:
    /// throw new NotFoundException();
    /// 
    /// KẾT QUẢ:
    /// - Message: empty string
    /// - Stack trace: có
    /// 
    /// LƯU Ý:
    /// - Ít khi dùng
    /// - Nên dùng constructor có message
    /// </summary>
    public NotFoundException()
        : base()
    {
    }

    /// <summary>
    /// Constructor với MESSAGE tùy chỉnh
    /// 
    /// SỬ DỤNG:
    /// throw new NotFoundException("User not found");
    /// 
    /// VÍ DỤ:
    /// var user = await _repository.FirstOrDefaultAsync(u => u.Email == email);
    /// if (user == null)
    ///     throw new NotFoundException($"User with email {email} not found");
    /// 
    /// KẾT QUẢ:
    /// Status: 404
    /// Message: "User with email john@example.com not found"
    /// </summary>
    /// <param name="message">Thông báo lỗi tùy chỉnh</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Constructor với ENTITY NAME và KEY (khuyến khích dùng)
    /// 
    /// GIẢI THÍCH:
    /// - Tự động tạo message theo format chuẩn
    /// - Format: "Entity \"{name}\" ({key}) was not found."
    /// - Rõ ràng, dễ hiểu, consistent
    /// 
    /// VÍ DỤ SỬ DỤNG:
    /// 
    /// // User
    /// var user = await _repository.GetByIdAsync(userId);
    /// if (user == null)
    ///     throw new NotFoundException(nameof(User), userId);
    /// 
    /// // Output: "Entity \"User\" (3fa85f64-...) was not found."
    /// 
    /// // Product
    /// var product = await _repository.GetByIdAsync(productId);
    /// if (product == null)
    ///     throw new NotFoundException(nameof(Product), productId);
    /// 
    /// // Output: "Entity \"Product\" (12345) was not found."
    /// 
    /// // Order
    /// var order = await _repository.GetByIdAsync(orderId);
    /// if (order == null)
    ///     throw new NotFoundException(nameof(Order), orderId);
    /// 
    /// // Output: "Entity \"Order\" (ORD-2024-001) was not found."
    /// 
    /// TẠI SAO DÙNG nameof()?
    /// - Type-safe: Compiler check
    /// - Refactor-friendly: Rename class → tự update
    /// - Không hardcode string
    /// 
    /// // TỐT
    /// throw new NotFoundException(nameof(User), userId);
    /// 
    /// // XẤU
    /// throw new NotFoundException("User", userId); // Hardcode, typo risk
    /// 
    /// TYPES OF KEY:
    /// - Guid: "Entity \"User\" (3fa85f64-5717-4562-b3fc-2c963f66afa6) was not found."
    /// - int: "Entity \"Product\" (12345) was not found."
    /// - string: "Entity \"Order\" (ORD-2024-001) was not found."
    /// 
    /// EXCEPTION HANDLING:
    /// 
    /// try
    /// {
    ///     var user = await GetUserByIdAsync(userId);
    ///     // user guaranteed not null here
    /// }
    /// catch (NotFoundException ex)
    /// {
    ///     _logger.LogWarning(ex, "User not found");
    ///     return NotFound(new { message = ex.Message });
    /// }
    /// 
    /// MIDDLEWARE XỬ LÝ:
    /// 
    /// public class ExceptionHandlingMiddleware
    /// {
    ///     public async Task InvokeAsync(HttpContext context)
    ///     {
    ///         try
    ///         {
    ///             await _next(context);
    ///         }
    ///         catch (NotFoundException ex)
    ///         {
    ///             context.Response.StatusCode = 404;
    ///             await context.Response.WriteAsJsonAsync(new
    ///             {
    ///                 error = new
    ///                 {
    ///                     code = "NotFound",
    ///                     message = ex.Message
    ///                 }
    ///             });
    ///         }
    ///     }
    /// }
    /// 
    /// RESPONSE KẾT QUẢ:
    /// 
    /// Status: 404 Not Found
    /// {
    ///   "error": {
    ///     "code": "NotFound",
    ///     "message": "Entity \"User\" (3fa85f64-5717-4562-b3fc-2c963f66afa6) was not found."
    ///   }
    /// }
    /// </summary>
    /// <param name="name">Tên entity (User, Product, Order...)</param>
    /// <param name="key">ID/Key của entity (Guid, int, string...)</param>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
        // BREAKDOWN MESSAGE:
        // "Entity \"User\" (3fa85f64-5717-4562-b3fc-2c963f66afa6) was not found."
        //  ↑       ↑      ↑                                          ↑
        //  prefix  name   key                                        suffix
    }
}