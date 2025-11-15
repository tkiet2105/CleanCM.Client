namespace CleanCCM.Infrastructure.Security;

/// <summary>
/// CONFIGURATION CHO API SIGNATURE
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// API SIGNATURE LÀ GÌ?
/// - Chữ ký số để bảo vệ API requests
/// - Đảm bảo request không bị giả mạo (tampered)
/// - Chống replay attack (dùng lại request cũ)
/// 
/// CÁCH HOẠT ĐỘNG:
/// 
/// 1. CLIENT TẠO SIGNATURE:
///    data = "POST|/api/users|1705317600|{body}"
///    signature = HMAC-SHA256(data, secretKey)
///    
/// 2. CLIENT GỬI REQUEST:
///    POST /api/users
///    Headers:
///      X-Signature: abc123...
///      X-Timestamp: 1705317600
///    Body: {...}
///    
/// 3. SERVER VALIDATE:
///    - Tính signature từ request
///    - So sánh với X-Signature
///    - Kiểm tra timestamp (không quá cũ)
///    - Nếu match → Accept
///    - Nếu không match → Reject (401)
/// 
/// KHI NÀO DÙNG?
/// - Public APIs (không dùng JWT)
/// - Webhook callbacks
/// - Server-to-server communication
/// - Mobile apps (bảo vệ API key)
/// 
/// VÍ DỤ:
/// // appsettings.json
/// {
///   "ApiSignatureSettings": {
///     "Enabled": true,
///     "SecretKey": "your-secret-key-here",
///     "ExpirySeconds": 300,
///     "ExcludedPaths": ["/api/auth/login", "/api/health"]
///   }
/// }
/// </summary>
public class ApiSignatureSettings
{
    /// <summary>
    /// SECTION NAME trong appsettings.json
    /// </summary>
    public const string SectionName = "ApiSignatureSettings";

    /// <summary>
    /// BẬT/TẮT signature validation
    /// 
    /// GIẢI THÍCH:
    /// - true: Bắt buộc signature cho mọi request
    /// - false: Tắt signature validation
    /// 
    /// KHI NÀO TẮT?
    /// - Development/Testing (dễ test)
    /// - Internal APIs (không cần signature)
    /// - Đã dùng JWT authentication
    /// 
    /// LƯU Ý:
    /// - Production: NÊN bật nếu có public APIs
    /// - Có thể dùng cả JWT + Signature (defense in depth)
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// SECRET KEY để tạo signature
    /// 
    /// GIẢI THÍCH:
    /// - Key dùng cho HMAC-SHA256
    /// - Client và Server PHẢI dùng CÙNG KEY
    /// - Tương tự như JWT Secret
    /// 
    /// BẢO MẬT:
    /// - PHẢI giữ BÍ MẬT tuyệt đối
    /// - Độ dài: Tối thiểu 32 characters
    /// - KHÔNG hardcode trong code
    /// - Dùng Environment Variable hoặc Secrets Manager
    /// 
    /// VÍ DỤ:
    /// SecretKey = "YourSuperSecretKeyForSignature123!@#$%"
    /// 
    /// LƯU Ý:
    /// - Client cần có key này để tạo signature
    /// - Nếu key lộ → attacker có thể tạo signature giả
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// THỜI GIAN HẾT HẠN của signature (seconds)
    /// 
    /// GIẢI THÍCH:
    /// - Signature chỉ valid trong khoảng thời gian này
    /// - Nếu timestamp quá cũ → reject (chống replay attack)
    /// 
    /// VÍ DỤ:
    /// ExpirySeconds = 300 (5 phút)
    /// 
    /// FLOW:
    /// 1. Client tạo timestamp: 1705317600 (10:00:00)
    /// 2. Client gửi request: 10:02:00 (2 phút sau)
    /// 3. Server check: |10:02:00 - 10:00:00| = 120s < 300s → OK
    /// 4. Attacker replay: 10:07:00 (7 phút sau)
    /// 5. Server check: |10:07:00 - 10:00:00| = 420s > 300s → REJECT
    /// 
    /// CHỌN GIÁ TRỊ:
    /// - 60s: Rất strict, dễ bị clock skew issues
    /// - 300s (5 phút): Balanced (recommended)
    /// - 600s (10 phút): Loose, ít bảo mật hơn
    /// </summary>
    public int ExpirySeconds { get; set; } = 300;

    /// <summary>
    /// DANH SÁCH PATHS KHÔNG CẦN SIGNATURE
    /// 
    /// GIẢI THÍCH:
    /// - Các endpoints được miễn signature validation
    /// - Thường là public endpoints không nhạy cảm
    /// 
    /// VÍ DỤ:
    /// ExcludedPaths = [
    ///     "/api/auth/login",    // Login không cần signature
    ///     "/api/health",        // Health check
    ///     "/api/public",        // Public APIs
    ///     "/swagger"            // Swagger UI
    /// ]
    /// 
    /// KHI NÀO EXCLUDE?
    /// - Login endpoints (chưa có credentials)
    /// - Health checks
    /// - Public read-only APIs
    /// - Documentation endpoints
    /// 
    /// LƯU Ý:
    /// - Càng ít exclude càng tốt
    /// - Chỉ exclude những gì thực sự cần thiết
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new();
}