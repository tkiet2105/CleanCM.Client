using MediatR;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Auth.DTOs;

namespace CleanCCM.Application.Features.Auth.Commands.Login;

/// <summary>
/// COMMAND ĐỂ ĐĂNG NHẬP
/// 
/// GIẢI THÍCH:
/// - Command đơn giản hơn Register
/// - Chỉ cần Email và Password
/// - Trả về AuthResponse (chứa tokens)
/// 
/// AUTH RESPONSE:
/// {
///   "accessToken": "eyJhbGc...",      // JWT token
///   "refreshToken": "xyz123...",       // Refresh token
///   "expiresIn": 3600,                 // 1 hour
///   "userId": "3fa85f64...",
///   "email": "user@example.com",
///   "userName": "john_doe"
/// }
/// 
/// JWT (JSON Web Token):
/// - Chứa user claims (id, email, roles...)
/// - Signed bởi server secret
/// - Client lưu và gửi trong header: Authorization: Bearer {token}
/// - Expire sau 1 giờ (configurable)
/// 
/// REFRESH TOKEN:
/// - Token để lấy access token mới
/// - Expire sau 7 ngày (configurable)
/// - Lưu trong database (user.RefreshToken)
/// - Dùng khi access token expire
/// 
/// FLOW:
/// 1. User login → Nhận access token + refresh token
/// 2. Dùng access token cho requests (1h)
/// 3. Access token expire → Dùng refresh token lấy token mới
/// 4. Refresh token expire → Phải login lại
/// </summary>
public record LoginCommand : IRequest<Result<AuthResponse>>
{
    /// <summary>
    /// EMAIL để đăng nhập
    /// 
    /// VALIDATION:
    /// - Required
    /// - Valid email format
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// PASSWORD
    /// 
    /// VALIDATION:
    /// - Required
    /// - (Không validate format vì đang login, không phải register)
    /// </summary>
    public string Password { get; init; } = string.Empty;
}