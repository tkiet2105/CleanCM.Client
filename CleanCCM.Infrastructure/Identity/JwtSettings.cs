namespace CleanCCM.Infrastructure.Identity;

/// <summary>
/// JWT CONFIGURATION SETTINGS
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// JWT SETTINGS LÀ GÌ?
/// - Configuration cho JWT token generation
/// - Load từ appsettings.json
/// - Secret key, issuer, audience, expiry...
/// 
/// CÁCH DÙNG:
/// 
/// // appsettings.json
/// {
///   "JwtSettings": {
///     "Secret": "your-secret-key-here...",
///     "Issuer": "CleanCCM",
///     "Audience": "CleanCCMUsers",
///     "ExpiryMinutes": 60,
///     "RefreshTokenExpiryDays": 7
///   }
/// }
/// 
/// // Program.cs
/// services.Configure<JwtSettings>(
///     configuration.GetSection(JwtSettings.SectionName)
/// );
/// 
/// // Service
/// private readonly JwtSettings _jwtSettings;
/// public JwtService(IOptions<JwtSettings> jwtSettings)
/// {
///     _jwtSettings = jwtSettings.Value;
/// }
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// SECTION NAME trong appsettings.json
    /// 
    /// VÍ DỤ:
    /// configuration.GetSection(JwtSettings.SectionName)
    /// → configuration.GetSection("JwtSettings")
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// SECRET KEY để sign JWT token
    /// 
    /// GIẢI THÍCH:
    /// - Dùng để tạo signature cho JWT
    /// - PHẢI giữ BÍ MẬT tuyệt đối
    /// - Nếu lộ → attacker có thể tạo token giả
    /// 
    /// YÊU CẦU:
    /// - Độ dài: Tối thiểu 32 characters (256 bits)
    /// - Random, phức tạp
    /// - KHÔNG hardcode trong code
    /// - Dùng Environment Variable hoặc Secrets Manager
    /// 
    /// VÍ DỤ:
    /// Secret = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%"
    /// 
    /// ⚠️ BẢO MẬT:
    /// - KHÔNG commit vào Git
    /// - KHÔNG share cho ai
    /// - ĐỔI KEY định kỳ (rotation)
    /// - Dùng Azure Key Vault / AWS Secrets Manager trong production
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// ISSUER - Ai phát hành token
    /// 
    /// GIẢI THÍCH:
    /// - "iss" claim trong JWT
    /// - Tên của application/service phát hành
    /// - Dùng để validate token từ đúng source
    /// 
    /// VÍ DỤ:
    /// Issuer = "CleanCCM"
    /// Issuer = "MyCompany.AuthService"
    /// 
    /// JWT CLAIM:
    /// {
    ///   "iss": "CleanCCM",
    ///   ...
    /// }
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// AUDIENCE - Ai sẽ dùng token này
    /// 
    /// GIẢI THÍCH:
    /// - "aud" claim trong JWT
    /// - Xác định token dành cho application/service nào
    /// - Dùng để validate token đúng audience
    /// 
    /// VÍ DỤ:
    /// Audience = "CleanCCMUsers"
    /// Audience = "MyCompany.WebApp"
    /// 
    /// JWT CLAIM:
    /// {
    ///   "aud": "CleanCCMUsers",
    ///   ...
    /// }
    /// 
    /// MULTIPLE AUDIENCES:
    /// Một token có thể có nhiều audiences
    /// Audience = "CleanCCMWeb,CleanCCMMobile"
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// THỜI GIAN HẾT HẠN của ACCESS TOKEN (phút)
    /// 
    /// GIẢI THÍCH:
    /// - Số phút token còn valid
    /// - Default: 60 phút (1 giờ)
    /// - Sau expire → phải dùng refresh token
    /// 
    /// VÍ DỤ:
    /// ExpiryMinutes = 60   // 1 hour
    /// ExpiryMinutes = 1440 // 24 hours
    /// ExpiryMinutes = 5    // 5 minutes (testing)
    /// 
    /// JWT CLAIM:
    /// {
    ///   "exp": 1705321200,  // Unix timestamp
    ///   ...
    /// }
    /// 
    /// BEST PRACTICES:
    /// - Short-lived: 15-60 phút
    /// - Càng ngắn càng an toàn
    /// - Balance giữa security và UX
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// THỜI GIAN HẾT HẠN của REFRESH TOKEN (ngày)
    /// 
    /// GIẢI THÍCH:
    /// - Số ngày refresh token còn valid
    /// - Default: 7 ngày
    /// - Sau expire → phải login lại
    /// 
    /// VÍ DỤ:
    /// RefreshTokenExpiryDays = 7   // 1 week
    /// RefreshTokenExpiryDays = 30  // 1 month
    /// RefreshTokenExpiryDays = 90  // 3 months
    /// 
    /// BEST PRACTICES:
    /// - Long-lived: 7-30 ngày
    /// - Balance giữa security và UX
    /// - Có thể revoke bất cứ lúc nào
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;
}