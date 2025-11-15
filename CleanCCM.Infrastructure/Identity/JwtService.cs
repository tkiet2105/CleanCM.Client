using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanCCM.Infrastructure.Identity;

/// <summary>
/// JWT SERVICE - Generate và validate JWT tokens
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// JWT SERVICE LÀM GÌ?
/// - Generate access tokens (JWT)
/// - Generate refresh tokens (random string)
/// - Validate tokens
/// - Extract claims từ tokens
/// 
/// JWT TOKEN LÀ GÌ?
/// - JSON Web Token
/// - 3 phần: Header.Payload.Signature
/// - Self-contained (chứa user info)
/// - Stateless (không cần lưu server)
/// 
/// CẤU TRÚC JWT:
/// 
/// eyJhbGc...  .  eyJuYW1l...  .  dGVzdF9zaWc...
///    ↓              ↓              ↓
/// HEADER        PAYLOAD       SIGNATURE
/// 
/// HEADER (Base64):
/// {
///   "alg": "HS256",      // Algorithm: HMAC-SHA256
///   "typ": "JWT"         // Type: JWT
/// }
/// 
/// PAYLOAD (Base64) - Claims:
/// {
///   "nameid": "user-id",         // User ID
///   "unique_name": "username",   // Username
///   "email": "user@example.com", // Email
///   "role": "User",              // Role
///   "nbf": 1705317600,           // Not Before
///   "exp": 1705321200,           // Expiration
///   "iat": 1705317600            // Issued At
/// }
/// 
/// SIGNATURE (HMAC-SHA256):
/// HMACSHA256(
///   base64(header) + "." + base64(payload),
///   secret_key
/// )
/// 
/// TẠI SAO AN TOÀN?
/// - Signature đảm bảo không bị giả mạo
/// - Nếu payload bị sửa → signature không match → reject
/// - Secret key chỉ server biết
/// 
/// VÍ DỤ ATTACK:
/// Hacker lấy được token, sửa role từ "User" thành "Admin"
/// → Payload thay đổi
/// → Signature không match
/// → Server reject token
/// </summary>
public class JwtService
{
    /// <summary>
    /// JWT SETTINGS từ configuration
    /// </summary>
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Constructor - Inject JwtSettings
    /// </summary>
    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// GENERATE ACCESS TOKEN (JWT)
    /// 
    /// GIẢI THÍCH:
    /// - Tạo JWT token chứa user claims
    /// - Sign với secret key
    /// - Set expiration time
    /// 
    /// FLOW:
    /// 
    /// 1. TẠO CLAIMS:
    ///    - User ID (nameid)
    ///    - Username (unique_name)
    ///    - Email (email)
    ///    - Roles (role)
    /// 
    /// 2. TẠO SIGNING CREDENTIALS:
    ///    - Secret key
    ///    - HMAC-SHA256 algorithm
    /// 
    /// 3. TẠO TOKEN DESCRIPTOR:
    ///    - Subject (claims)
    ///    - Issuer, Audience
    ///    - Expiration
    ///    - Signing credentials
    /// 
    /// 4. CREATE TOKEN:
    ///    - JwtSecurityTokenHandler.CreateToken()
    ///    - Return JWT string
    /// 
    /// VÍ DỤ SỬ DỤNG:
    /// 
    /// var token = _jwtService.GenerateAccessToken(user, roles);
    /// 
    /// // Token:
    /// "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJ1bmlxdWVfbmFtZSI6ImpvaG5fZG9lIiwiZW1haWwiOiJ1c2VyQGV4YW1wbGUuY29tIiwicm9sZSI6IlVzZXIiLCJuYmYiOjE3MDUzMTc2MDAsImV4cCI6MTcwNTMyMTIwMCwiaWF0IjoxNzA1MzE3NjAwfQ.dGVzdF9zaWduYXR1cmVfaGVyZQ"
    /// 
    /// DECODE TOKEN (jwt.io):
    /// {
    ///   "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "unique_name": "john_doe",
    ///   "email": "user@example.com",
    ///   "role": "User",
    ///   "exp": 1705321200
    /// }
    /// </summary>
    /// <param name="user">ApplicationUser object</param>
    /// <param name="roles">Danh sách roles của user</param>
    /// <returns>JWT token string</returns>
    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        // ========== BƯỚC 1: TẠO CLAIMS ==========

        /// <summary>
        /// CLAIMS LÀ GÌ?
        /// - Key-value pairs chứa thông tin user
        /// - Embed trong JWT payload
        /// - Server extract claims để authorize
        /// 
        /// STANDARD CLAIMS:
        /// - nameid: User ID
        /// - unique_name: Username
        /// - email: Email
        /// - role: Role(s)
        /// - jti: JWT ID (unique token identifier)
        /// 
        /// CUSTOM CLAIMS (có thể thêm):
        /// - given_name: FirstName
        /// - family_name: LastName
        /// - phone_number: PhoneNumber
        /// - company: CompanyId
        /// 
        /// VÍ DỤ THÊM CUSTOM CLAIM:
        /// claims.Add(new Claim("company_id", user.CompanyId.ToString()));
        /// </summary>
        var claims = new List<Claim>
        {
            // USER ID (Primary key)
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
            
            // USERNAME
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            
            // EMAIL
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            
            // JWT ID (Unique token identifier)
            // Dùng để track token, revoke token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
            // ISSUED AT (Unix timestamp)
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // THÊM ROLES VÀO CLAIMS
        // Một user có thể có nhiều roles
        // VÍ DỤ: roles = ["User", "Manager"]
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // KẾT QUẢ CLAIMS:
        // [
        //   { "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
        //   { "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
        //   { "unique_name": "john_doe" },
        //   { "email": "user@example.com" },
        //   { "jti": "7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b" },
        //   { "iat": "1705317600" },
        //   { "role": "User" },
        //   { "role": "Manager" }
        // ]

        // ========== BƯỚC 2: TẠO SIGNING CREDENTIALS ==========

        /// <summary>
        /// SIGNING CREDENTIALS LÀ GÌ?
        /// - Secret key để sign token
        /// - Algorithm: HMAC-SHA256
        /// 
        /// TẠI SAO CẦN SIGN?
        /// - Đảm bảo token không bị giả mạo
        /// - Server verify signature khi nhận token
        /// - Nếu payload bị sửa → signature invalid
        /// 
        /// SECRET KEY:
        /// - PHẢI giữ bí mật tuyệt đối
        /// - Tối thiểu 256 bits (32 characters)
        /// - KHÔNG hardcode trong code
        /// - Dùng Environment Variable
        /// </summary>
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ========== BƯỚC 3: TẠO TOKEN DESCRIPTOR ==========

        /// <summary>
        /// TOKEN DESCRIPTOR - Mô tả token sẽ tạo
        /// 
        /// PROPERTIES:
        /// - Subject: ClaimsIdentity (chứa claims)
        /// - Issuer: Ai phát hành token
        /// - Audience: Token dành cho ai
        /// - Expires: Thời gian hết hạn
        /// - SigningCredentials: Key và algorithm
        /// </summary>
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // SUBJECT (Claims)
            Subject = new ClaimsIdentity(claims),

            // ISSUER (Ai phát hành)
            // VÍ DỤ: "CleanCCM"
            Issuer = _jwtSettings.Issuer,

            // AUDIENCE (Token dành cho ai)
            // VÍ DỤ: "CleanCCMUsers"
            Audience = _jwtSettings.Audience,

            // EXPIRATION (Hết hạn sau bao lâu)
            // VÍ DỤ: DateTime.UtcNow.AddMinutes(60) → 1 giờ
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),

            // SIGNING CREDENTIALS (Key + Algorithm)
            SigningCredentials = credentials
        };

        // ========== BƯỚC 4: CREATE TOKEN ==========

        /// <summary>
        /// TẠO TOKEN
        /// - JwtSecurityTokenHandler: Class xử lý JWT
        /// - CreateToken(): Generate token từ descriptor
        /// - WriteToken(): Convert token object thành string
        /// </summary>
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // RETURN JWT STRING
        // VÍ DỤ:
        // "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzZmE4..."
        return tokenString;
    }

    /// <summary>
    /// GENERATE REFRESH TOKEN
    /// 
    /// GIẢI THÍCH:
    /// - Tạo random string
    /// - KHÔNG phải JWT (không có structure)
    /// - Lưu trong database
    /// - Dùng để lấy access token mới
    /// 
    /// TẠI SAO KHÔNG PHẢI JWT?
    /// - Không cần claims
    /// - Chỉ cần unique string
    /// - Random → secure
    /// - Phải check database khi validate
    /// 
    /// FLOW:
    /// 
    /// 1. GENERATE RANDOM BYTES (64 bytes):
    ///    var randomBytes = new byte[64];
    ///    RandomNumberGenerator.Fill(randomBytes);
    /// 
    /// 2. CONVERT TO BASE64:
    ///    return Convert.ToBase64String(randomBytes);
    /// 
    /// 3. RESULT:
    ///    "xyz789abc123def456ghi789jkl012mno345pqr678stu901vwx234yza567bcd890"
    /// 
    /// VÍ DỤ SỬ DỤNG:
    /// 
    /// var refreshToken = _jwtService.GenerateRefreshToken();
    /// user.RefreshToken = refreshToken;
    /// user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    /// await _userManager.UpdateAsync(user);
    /// 
    /// BẢO MẬT:
    /// - Cryptographically secure random
    /// - 64 bytes = 512 bits
    /// - Không thể guess/brute-force
    /// </summary>
    /// <returns>Random refresh token string (Base64)</returns>
    public string GenerateRefreshToken()
    {
        // TẠO RANDOM BYTES (64 bytes)
        var randomBytes = new byte[64];

        // SỬ DỤNG RandomNumberGenerator (cryptographically secure)
        // KHÔNG dùng Random() (không secure)
        RandomNumberGenerator.Fill(randomBytes);

        // CONVERT TO BASE64 STRING
        // VÍ DỤ OUTPUT:
        // "xyz789abc123def456ghi789jkl012mno345pqr678stu901vwx234yza567bcd890="
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// GET PRINCIPAL FROM EXPIRED TOKEN
    /// 
    /// GIẢI THÍCH:
    /// - Extract claims từ token ĐÃ EXPIRE
    /// - Dùng khi refresh token
    /// - Không validate expiration (vì token đã expire)
    /// - Validate signature và format
    /// 
    /// TẠI SAO CẦN?
    /// - Khi refresh token, access token đã expire
    /// - Cần extract userId từ expired token
    /// - Find user trong database
    /// - Generate new tokens
    /// 
    /// FLOW REFRESH TOKEN:
    /// 
    /// 1. CLIENT GỬI:
    ///    {
    ///      "accessToken": "expired_token...",
    ///      "refreshToken": "xyz789..."
    ///    }
    /// 
    /// 2. SERVER EXTRACT USER ID:
    ///    var principal = GetPrincipalFromExpiredToken(accessToken);
    ///    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /// 
    /// 3. FIND USER:
    ///    var user = await _userManager.FindByIdAsync(userId);
    /// 
    /// 4. VALIDATE REFRESH TOKEN:
    ///    if (user.RefreshToken != refreshToken)
    ///        return Error.Unauthorized();
    /// 
    /// 5. GENERATE NEW TOKENS:
    ///    var newAccessToken = GenerateAccessToken(user, roles);
    ///    var newRefreshToken = GenerateRefreshToken();
    /// 
    /// TOKEN VALIDATION PARAMETERS:
    /// - ValidateIssuerSigningKey: true (validate signature)
    /// - ValidateIssuer: true (check issuer)
    /// - ValidateAudience: true (check audience)
    /// - ValidateLifetime: FALSE (KHÔNG check expiration - vì token đã expire)
    /// </summary>
    /// <param name="token">Expired access token</param>
    /// <returns>ClaimsPrincipal chứa claims từ token</returns>
    /// <exception cref="SecurityTokenException">Token invalid</exception>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // TẠO TOKEN VALIDATION PARAMETERS
        var tokenValidationParameters = new TokenValidationParameters
        {
            // VALIDATE SIGNATURE
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),

            // VALIDATE ISSUER
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,

            // VALIDATE AUDIENCE
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,

            // ⚠️ KHÔNG VALIDATE LIFETIME (cho phép expired token)
            ValidateLifetime = false
        };

        // VALIDATE TOKEN VÀ EXTRACT PRINCIPAL
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            token,
            tokenValidationParameters,
            out SecurityToken securityToken
        );

        // KIỂM TRA TOKEN LÀ JWT VỚI ĐÚNG ALGORITHM
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        // RETURN CLAIMS PRINCIPAL
        // VÍ DỤ:
        // principal.Claims = [
        //   { "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
        //   { "unique_name": "john_doe" },
        //   { "email": "user@example.com" },
        //   { "role": "User" }
        // ]
        return principal;
    }
}