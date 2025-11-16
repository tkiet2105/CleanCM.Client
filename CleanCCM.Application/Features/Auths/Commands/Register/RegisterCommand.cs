using MediatR;
using CleanCCM.Application.Common.Models;

namespace CleanCCM.Application.Features.Auth.Commands.Register;

/// <summary>
/// COMMAND ĐỂ ĐĂNG KÝ USER MỚI
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// COMMAND LÀ GÌ?
/// - Đại diện cho MỘT HÀNH ĐỘNG (action/intent)
/// - Thay đổi state của system (write operation)
/// - Ví dụ: Register, Login, CreateOrder, UpdateProduct
/// 
/// COMMAND vs QUERY (CQRS Pattern):
/// 
/// COMMAND:
/// - Thay đổi dữ liệu (Insert/Update/Delete)
/// - Không trả về data (hoặc trả về ID)
/// - Ví dụ: CreateUser, UpdateProfile, DeleteOrder
/// 
/// QUERY:
/// - Chỉ ĐỌC dữ liệu (Select)
/// - Không thay đổi state
/// - Ví dụ: GetUserById, GetAllProducts, SearchOrders
/// 
/// MEDIATR REQUEST:
/// - RegisterCommand implement IRequest<Result<string>>
/// - IRequest<TResponse>: Marker interface cho MediatR
/// - TResponse: Type của response (Result<string> trong trường hợp này)
/// 
/// FLOW HOẠT ĐỘNG:
/// 
/// 1. CLIENT GỬI REQUEST:
///    POST /api/auth/register
///    {
///      "email": "user@example.com",
///      "userName": "john_doe",
///      "password": "SecurePass123!",
///      "confirmPassword": "SecurePass123!",
///      "firstName": "John",
///      "lastName": "Doe"
///    }
/// 
/// 2. CONTROLLER TẠO COMMAND:
///    var command = new RegisterCommand
///    {
///        Email = request.Email,
///        UserName = request.UserName,
///        ...
///    };
/// 
/// 3. GỬI QUA MEDIATR:
///    var result = await _mediator.Send(command);
/// 
/// 4. MEDIATR PIPELINE:
///    Command
///      → ValidationBehaviour (validate)
///      → PerformanceBehaviour (measure time)
///      → RegisterCommandHandler (execute)
///      → Result<string> (userId)
/// 
/// 5. CONTROLLER TRẢ VỀ:
///    if (result.IsSuccess)
///        return Ok(new { userId = result.Value });
///    else
///        return BadRequest(result.Error);
/// 
/// TẠI SAO DÙNG RECORD?
/// - Immutable (không thay đổi sau khi tạo)
/// - Value-based equality
/// - Concise syntax (init-only properties)
/// - Thread-safe
/// 
/// VÍ DỤ:
/// // Record
/// public record RegisterCommand
/// {
///     public string Email { get; init; }
/// }
/// 
/// var cmd = new RegisterCommand { Email = "test@example.com" };
/// cmd.Email = "new@example.com"; // COMPILE ERROR - init only
/// 
/// // Class (so sánh)
/// public class RegisterRequest
/// {
///     public string Email { get; set; }
/// }
/// 
/// var req = new RegisterRequest { Email = "test@example.com" };
/// req.Email = "new@example.com"; // OK - có thể thay đổi
/// </summary>
public record RegisterCommand : IRequest<Result<string>>
{
    /// <summary>
    /// EMAIL của user mới
    /// 
    /// VALIDATION:
    /// - Required (bắt buộc)
    /// - Valid email format
    /// - Unique (không trùng trong DB)
    /// 
    /// VÍ DỤ:
    /// Email = "user@example.com" ✓
    /// Email = "invalid-email" ✗ (sai format)
    /// Email = "" ✗ (required)
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// USERNAME để đăng nhập
    /// 
    /// VALIDATION:
    /// - Required
    /// - Length: 3-50 characters
    /// - Unique (không trùng)
    /// - Alphanumeric + underscore
    /// 
    /// VÍ DỤ:
    /// UserName = "john_doe" ✓
    /// UserName = "jd" ✗ (quá ngắn)
    /// UserName = "john doe" ✗ (có space)
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// PASSWORD
    /// 
    /// VALIDATION:
    /// - Required
    /// - Min length: 8
    /// - Must contain: uppercase, lowercase, number, special char
    /// 
    /// VÍ DỤ:
    /// Password = "SecurePass123!" ✓
    /// Password = "password" ✗ (không có uppercase, number, special)
    /// Password = "Pass1!" ✗ (quá ngắn)
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// XÁC NHẬN PASSWORD
    /// 
    /// VALIDATION:
    /// - Required
    /// - Must equal Password
    /// 
    /// VÍ DỤ:
    /// Password = "SecurePass123!"
    /// ConfirmPassword = "SecurePass123!" ✓
    /// ConfirmPassword = "DifferentPass" ✗ (không khớp)
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// TÊN (optional)
    /// 
    /// VALIDATION:
    /// - Optional (có thể null/empty)
    /// - Max length: 100
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// HỌ (optional)
    /// 
    /// VALIDATION:
    /// - Optional
    /// - Max length: 100
    /// </summary>
    public string? LastName { get; init; }
}