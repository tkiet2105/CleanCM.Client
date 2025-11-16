namespace CleanCCM.Application.Features.Auth.DTOs;

/// <summary>
/// RESPONSE TRẢ VỀ SAU KHI LOGIN/REFRESH TOKEN THÀNH CÔNG
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// AUTH RESPONSE LÀ GÌ?
/// - DTO (Data Transfer Object) chứa thông tin authentication
/// - Trả về sau login hoặc refresh token thành công
/// - Client lưu và dùng tokens này cho các requests tiếp theo
/// 
/// JWT AUTHENTICATION FLOW:
/// 
/// 1. LOGIN:
///    Client → POST /api/auth/login { email, password }
///    Server → AuthResponse { accessToken, refreshToken, ... }
/// 
/// 2. LƯU TOKENS:
///    localStorage.setItem('accessToken', response.accessToken);
///    localStorage.setItem('refreshToken', response.refreshToken);
/// 
/// 3. GỬI REQUEST VỚI TOKEN:
///    GET /api/users/me
///    Headers: {
///      Authorization: "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
///    }
/// 
/// 4. TOKEN EXPIRE (sau 1h):
///    Server → 401 Unauthorized { message: "Token expired" }
/// 
/// 5. REFRESH TOKEN:
///    Client → POST /api/auth/refresh-token { accessToken, refreshToken }
///    Server → AuthResponse { accessToken: NEW, refreshToken: NEW, ... }
/// 
/// 6. LƯU TOKENS MỚI:
///    localStorage.setItem('accessToken', response.accessToken);
///    localStorage.setItem('refreshToken', response.refreshToken);
/// 
/// VÍ DỤ RESPONSE JSON:
/// {
///   "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJ1bmlxdWVfbmFtZSI6ImpvaG5fZG9lIiwiZW1haWwiOiJ1c2VyQGV4YW1wbGUuY29tIiwicm9sZSI6IlVzZXIiLCJuYmYiOjE3MDUzMTc2MDAsImV4cCI6MTcwNTMyMTIwMCwiaWF0IjoxNzA1MzE3NjAwfQ.dGVzdF9zaWduYXR1cmVfaGVyZQ",
///   "refreshToken": "xyz789abc123def456ghi789jkl012mno345pqr678stu901vwx234yza567bcd890",
///   "expiresIn": 3600,
///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///   "email": "user@example.com",
///   "userName": "john_doe"
/// }
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// ACCESS TOKEN (JWT) - Token để xác thực requests
    /// 
    /// GIẢI THÍCH:
    /// - JSON Web Token (JWT)
    /// - Chứa user claims (id, email, roles...)
    /// - Signed bởi server secret key
    /// - Client gửi trong header: Authorization: Bearer {token}
    /// 
    /// CẤU TRÚC JWT (3 phần, ngăn cách bởi dấu chấm):
    /// 
    /// eyJhbGc...  .  eyJuYW1l...  .  dGVzdF9zaWc...
    ///    ↑              ↑              ↑
    ///  Header        Payload       Signature
    /// 
    /// HEADER (Base64):
    /// {
    ///   "alg": "HS256",
    ///   "typ": "JWT"
    /// }
    /// 
    /// PAYLOAD (Base64) - Claims:
    /// {
    ///   "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // UserId
    ///   "unique_name": "john_doe",                          // UserName
    ///   "email": "user@example.com",                        // Email
    ///   "role": "User",                                     // Role
    ///   "nbf": 1705317600,                                  // Not Before
    ///   "exp": 1705321200,                                  // Expiration (1h)
    ///   "iat": 1705317600                                   // Issued At
    /// }
    /// 
    /// SIGNATURE (HMAC-SHA256):
    /// HMACSHA256(
    ///   base64UrlEncode(header) + "." + base64UrlEncode(payload),
    ///   secret
    /// )
    /// 
    /// BẢO MẬT:
    /// - Server verify signature để đảm bảo token không bị giả mạo
    /// - Nếu payload bị thay đổi → signature không match → reject
    /// - Secret key PHẢI giữ bí mật
    /// 
    /// EXPIRE:
    /// - Default: 1 giờ (3600 seconds)
    /// - Configurable trong appsettings.json
    /// - Sau khi expire → phải dùng refresh token
    /// 
    /// CÁCH DÙNG TRONG CLIENT:
    /// 
    /// // JavaScript/TypeScript
    /// const token = response.accessToken;
    /// localStorage.setItem('accessToken', token);
    /// 
    /// // Gửi request với token
    /// fetch('/api/users/me', {
    ///   headers: {
    ///     'Authorization': `Bearer ${token}`
    ///   }
    /// });
    /// 
    /// // C# HttpClient
    /// var token = response.AccessToken;
    /// httpClient.DefaultRequestHeaders.Authorization = 
    ///     new AuthenticationHeaderValue("Bearer", token);
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// REFRESH TOKEN - Token để lấy access token mới
    /// 
    /// GIẢI THÍCH:
    /// - Random string (không phải JWT)
    /// - Lưu trong database (User.RefreshToken)
    /// - Expire sau 7 ngày (configurable)
    /// - Dùng để lấy access token mới khi expire
    /// 
    /// TẠI SAO CẦN REFRESH TOKEN?
    /// 
    /// 1. BẢO MẬT:
    ///    - Access token expire nhanh (1h) → giảm thiểu rủi ro nếu bị đánh cắp
    ///    - Refresh token expire lâu (7 days) → UX tốt, không cần login liên tục
    ///    - Refresh token có thể revoke (xóa trong DB)
    /// 
    /// 2. USER EXPERIENCE:
    ///    - User không cần login lại mỗi 1h
    ///    - Client tự động refresh token khi cần
    ///    - Seamless experience
    /// 
    /// FLOW REFRESH TOKEN:
    /// 
    /// 1. Access token expire:
    ///    Request → 401 Unauthorized
    /// 
    /// 2. Client detect 401:
    ///    if (response.status === 401) {
    ///      // Try refresh token
    ///    }
    /// 
    /// 3. Gửi refresh request:
    ///    POST /api/auth/refresh-token
    ///    {
    ///      "accessToken": "expired_token...",
    ///      "refreshToken": "xyz789..."
    ///    }
    /// 
    /// 4. Server xử lý:
    ///    - Extract userId từ expired access token
    ///    - Find user trong database
    ///    - Check user.RefreshToken === request.RefreshToken
    ///    - Check refresh token chưa expire
    ///    - Generate NEW access token + NEW refresh token
    ///    - Update user.RefreshToken trong database
    ///    - Return AuthResponse mới
    /// 
    /// 5. Client lưu tokens mới:
    ///    localStorage.setItem('accessToken', newAccessToken);
    ///    localStorage.setItem('refreshToken', newRefreshToken);
    /// 
    /// 6. Retry request ban đầu:
    ///    // Request lại với token mới
    /// 
    /// BẢO MẬT:
    /// - Mỗi lần refresh → token cũ bị invalidate
    /// - Refresh token có thể revoke (logout all devices)
    /// - Lưu trong database → có thể check và xóa
    /// 
    /// GENERATION:
    /// var randomBytes = new byte[64];
    /// using var rng = RandomNumberGenerator.Create();
    /// rng.GetBytes(randomBytes);
    /// return Convert.ToBase64String(randomBytes);
    /// 
    /// STORAGE:
    /// // Client
    /// localStorage.setItem('refreshToken', token);
    /// 
    /// // Server (Database)
    /// user.RefreshToken = token;
    /// user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// EXPIRE TIME - Thời gian token còn hiệu lực (seconds)
    /// 
    /// GIẢI THÍCH:
    /// - Số giây token còn valid
    /// - Default: 3600 seconds (1 hour)
    /// - Client dùng để tính thời điểm cần refresh
    /// 
    /// VÍ DỤ:
    /// ExpiresIn = 3600  // 1 hour
    /// ExpiresIn = 7200  // 2 hours
    /// ExpiresIn = 300   // 5 minutes
    /// 
    /// CLIENT XỬ LÝ:
    /// 
    /// // JavaScript
    /// const expiresAt = Date.now() + (response.expiresIn * 1000);
    /// localStorage.setItem('tokenExpiresAt', expiresAt);
    /// 
    /// // Check token còn valid không
    /// const isTokenValid = Date.now() < expiresAt;
    /// 
    /// // Auto refresh trước khi expire (5 phút trước)
    /// const shouldRefresh = Date.now() > (expiresAt - 5 * 60 * 1000);
    /// if (shouldRefresh) {
    ///   await refreshToken();
    /// }
    /// 
    /// C# CLIENT:
    /// var expiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
    /// // Lưu expiresAt để check sau
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// USER ID của user đã login
    /// 
    /// GIẢI THÍCH:
    /// - GUID của user trong database
    /// - Dùng để identify user
    /// - Client có thể cache để tránh decode JWT
    /// 
    /// VÍ DỤ:
    /// UserId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// 
    /// CÁCH DÙNG:
    /// // Lưu userId để dùng
    /// localStorage.setItem('userId', response.userId);
    /// 
    /// // Fetch user profile
    /// fetch(`/api/users/${response.userId}`)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// EMAIL của user
    /// 
    /// GIẢI THÍCH:
    /// - Email của user đã login
    /// - Có thể hiển thị trong UI
    /// 
    /// VÍ DỤ:
    /// Email = "user@example.com"
    /// 
    /// UI:
    /// Welcome, user@example.com
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// USERNAME của user
    /// 
    /// GIẢI THÍCH:
    /// - Username để hiển thị
    /// - Có thể khác với email
    /// 
    /// VÍ DỤ:
    /// UserName = "john_doe"
    /// 
    /// UI:
    /// Welcome back, john_doe!
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}