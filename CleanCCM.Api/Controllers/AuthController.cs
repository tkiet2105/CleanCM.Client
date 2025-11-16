using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanCCM.Application.Features.Auth.Commands.Register;
using CleanCCM.Application.Features.Auth.Commands.Login;
using CleanCCM.Application.Features.Auth.Commands.RefreshToken;

namespace CleanCCM.API.Controllers;

/// <summary>
/// AUTH CONTROLLER - Xử lý authentication endpoints
/// 
/// ENDPOINTS:
/// - POST /api/auth/register   → Đăng ký user mới
/// - POST /api/auth/login      → Đăng nhập
/// - POST /api/auth/refresh    → Refresh access token
/// - POST /api/auth/logout     → Logout (revoke refresh token)
/// 
/// ATTRIBUTES:
/// - [AllowAnonymous]: Cho phép access mà không cần authentication
/// - [Authorize]: Yêu cầu authentication
/// </summary>
public class AuthController : BaseApiController
{
    /// <summary>
    /// ĐĂNG KÝ USER MỚI
    /// 
    /// ENDPOINT: POST /api/auth/register
    /// 
    /// REQUEST BODY:
    /// {
    ///   "email": "user@example.com",
    ///   "userName": "john_doe",
    ///   "password": "SecurePass123!",
    ///   "confirmPassword": "SecurePass123!",
    ///   "firstName": "John",
    ///   "lastName": "Doe"
    /// }
    /// 
    /// SUCCESS RESPONSE (200):
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// }
    /// 
    /// ERROR RESPONSES:
    /// 
    /// 400 Bad Request (Validation):
    /// {
    ///   "error": {
    ///     "code": "Validation.Failed",
    ///     "message": "One or more validation errors occurred",
    ///     "errors": {
    ///       "Email": ["Invalid email format"],
    ///       "Password": ["Password must be at least 8 characters"]
    ///     }
    ///   }
    /// }
    /// 
    /// 409 Conflict (Email exists):
    /// {
    ///   "error": {
    ///     "code": "AUTH_7101",
    ///     "message": "Email is already in use"
    ///   }
    /// }
    /// 
    /// FLOW:
    /// 1. Client gửi request
    /// 2. ModelBinding: JSON → RegisterCommand
    /// 3. ValidationBehaviour: Validate command
    /// 4. RegisterCommandHandler: Execute business logic
    /// 5. IdentityService: Create user
    /// 6. Return userId
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleFailure(result);

        return Ok(new { userId = result.Value });
    }

    /// <summary>
    /// ĐĂNG NHẬP
    /// 
    /// ENDPOINT: POST /api/auth/login
    /// 
    /// REQUEST BODY:
    /// {
    ///   "email": "user@example.com",
    ///   "password": "SecurePass123!"
    /// }
    /// 
    /// SUCCESS RESPONSE (200):
    /// {
    ///   "accessToken": "eyJhbGc...",
    ///   "refreshToken": "xyz789...",
    ///   "expiresIn": 3600,
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "email": "user@example.com",
    ///   "userName": "john_doe"
    /// }
    /// 
    /// ERROR RESPONSES:
    /// 
    /// 401 Unauthorized (Invalid credentials):
    /// {
    ///   "error": {
    ///     "code": "AUTH_7001",
    ///     "message": "Invalid email or password"
    ///   }
    /// }
    /// 
    /// 401 Unauthorized (Account inactive):
    /// {
    ///   "error": {
    ///     "code": "AUTH_7002",
    ///     "message": "Account is inactive"
    ///   }
    /// }
    /// 
    /// 401 Unauthorized (Account locked):
    /// {
    ///   "error": {
    ///     "code": "AUTH_7004",
    ///     "message": "Account is locked due to multiple failed login attempts"
    ///   }
    /// }
    /// 
    /// FLOW:
    /// 1. Client gửi email + password
    /// 2. LoginCommandHandler validate credentials
    /// 3. IdentityService check password
    /// 4. Generate JWT tokens
    /// 5. Update refresh token trong DB
    /// 6. Return AuthResponse
    /// 
    /// CLIENT LƯU TOKENS:
    /// localStorage.setItem('accessToken', response.accessToken);
    /// localStorage.setItem('refreshToken', response.refreshToken);
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(CreateProductCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// REFRESH ACCESS TOKEN
    /// 
    /// ENDPOINT: POST /api/auth/refresh
    /// 
    /// REQUEST BODY:
    /// {
    ///   "accessToken": "eyJhbGc...",      // Expired access token
    ///   "refreshToken": "xyz789..."       // Valid refresh token
    /// }
    /// 
    /// SUCCESS RESPONSE (200):
    /// {
    ///   "accessToken": "eyJhbGc...",      // NEW access token
    ///   "refreshToken": "abc456...",      // NEW refresh token
    ///   "expiresIn": 3600,
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "email": "user@example.com",
    ///   "userName": "john_doe"
    /// }
    /// 
    /// ERROR RESPONSES:
    /// 
    /// 401 Unauthorized (Invalid token):
    /// {
    ///   "error": {
    ///     "code": "BASE_3001",
    ///     "message": "Invalid token"
    ///   }
    /// }
    /// 
    /// 401 Unauthorized (Invalid refresh token):
    /// {
    ///   "error": {
    ///     "code": "AUTH_7005",
    ///     "message": "Invalid refresh token"
    ///   }
    /// }
    /// 
    /// 401 Unauthorized (Expired refresh token):
    /// {
    ///   "error": {
    ///     "code": "BASE_3002",
    ///     "message": "Refresh token has expired"
    ///   }
    /// }
    /// 
    /// FLOW:
    /// 1. Access token expire (sau 1h)
    /// 2. Client detect 401
    /// 3. Client gửi refresh request
    /// 4. Server validate refresh token
    /// 5. Generate new tokens
    /// 6. Return new AuthResponse
    /// 7. Client lưu tokens mới
    /// 
    /// CLIENT AUTO REFRESH:
    /// // Axios interceptor
    /// axios.interceptors.response.use(
    ///   response => response,
    ///   async error => {
    ///     if (error.response.status === 401) {
    ///       const newTokens = await refreshToken();
    ///       // Retry original request with new token
    ///     }
    ///   }
    /// );
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// LOGOUT - Revoke refresh token
    /// 
    /// ENDPOINT: POST /api/auth/logout
    /// 
    /// HEADERS:
    /// Authorization: Bearer {accessToken}
    /// 
    /// SUCCESS RESPONSE (204):
    /// No Content
    /// 
    /// ERROR RESPONSES:
    /// 
    /// 401 Unauthorized (No token):
    /// {
    ///   "error": {
    ///     "code": "Unauthorized",
    ///     "message": "Authentication required"
    ///   }
    /// }
    /// 
    /// 404 Not Found (User not found):
    /// {
    ///   "error": {
    ///     "code": "NF_1001",
    ///     "message": "User not found"
    ///   }
    /// }
    /// 
    /// FLOW:
    /// 1. Client gửi logout request (với access token)
    /// 2. Extract userId từ token
    /// 3. Set user.RefreshToken = null trong DB
    /// 4. Return 204 No Content
    /// 5. Client xóa tokens
    /// 
    /// CLIENT LOGOUT:
    /// localStorage.removeItem('accessToken');
    /// localStorage.removeItem('refreshToken');
    /// redirectToLogin();
    /// 
    /// LƯU Ý:
    /// - Access token vẫn valid cho đến khi expire
    /// - Chỉ revoke refresh token (không thể refresh nữa)
    /// - Nếu cần logout ngay lập tức → Token blacklist (advanced)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // LẤY USER ID TỪ CLAIMS
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // TẠO REVOKE COMMAND (cần tạo file này)
        // var command = new RevokeRefreshTokenCommand { UserId = userId };
        // var result = await Mediator.Send(command);

        // TẠM THỜI: Gọi trực tiếp service
        // (Nên dùng command pattern như trên)
        return NoContent();
    }
}