using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CleanCCM.Infrastructure.Security;

/// <summary>
/// API SIGNATURE SERVICE IMPLEMENTATION
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// SERVICE NÀY LÀM GÌ?
/// - Tạo và validate HMAC-SHA256 signatures
/// - Bảo vệ API requests khỏi tampering
/// - Chống replay attacks
/// 
/// HMAC-SHA256 LÀ GÌ?
/// - Hash-based Message Authentication Code
/// - Dùng SHA-256 hash algorithm
/// - Cần secret key để tạo và verify
/// 
/// TẠI SAO AN TOÀN?
/// - Không thể tạo signature hợp lệ nếu không có secret key
/// - Nếu data thay đổi → signature khác → reject
/// - Timestamp chống replay attack
/// 
/// FLOW HOÀN CHỈNH:
/// 
/// === CLIENT SIDE ===
/// 1. Chuẩn bị data:
///    method = "POST"
///    path = "/api/users"
///    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
///    body = "{\"email\":\"user@example.com\"}"
/// 
/// 2. Tạo signature:
///    signature = _signatureService.GenerateSignature(method, path, timestamp, body)
/// 
/// 3. Gửi request:
///    POST /api/users
///    Headers:
///      X-Timestamp: 1705317600
///      X-Signature: abc123def456...
///    Body: {"email":"user@example.com"}
/// 
/// === SERVER SIDE ===
/// 4. Extract headers:
///    timestamp = Request.Headers["X-Timestamp"]
///    signature = Request.Headers["X-Signature"]
/// 
/// 5. Validate timestamp:
///    if (!IsTimestampValid(timestamp, 300))
///        return Unauthorized("Signature expired")
/// 
/// 6. Validate signature:
///    if (!ValidateSignature(method, path, timestamp, signature, body))
///        return Unauthorized("Invalid signature")
/// 
/// 7. Process request:
///    // Request hợp lệ, tiếp tục xử lý
/// </summary>
public class ApiSignatureService : IApiSignatureService
{
    /// <summary>
    /// SETTINGS từ configuration
    /// </summary>
    private readonly ApiSignatureSettings _settings;

    /// <summary>
    /// Constructor - Inject settings
    /// </summary>
    public ApiSignatureService(IOptions<ApiSignatureSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// TẠO SIGNATURE CHO REQUEST
    /// 
    /// ALGORITHM: HMAC-SHA256
    /// 
    /// BƯỚC 1: TẠO DATA STRING
    /// - Format: "{method}|{path}|{timestamp}|{body}"
    /// - VÍ DỤ: "POST|/api/users|1705317600|{\"email\":\"user@example.com\"}"
    /// 
    /// TẠI SAO FORMAT NÀY?
    /// - method: Đảm bảo không thay đổi GET → POST
    /// - path: Đảm bảo không thay đổi endpoint
    /// - timestamp: Chống replay attack
    /// - body: Đảm bảo payload không bị sửa
    /// - Dấu | làm separator (ít xung đột)
    /// 
    /// BƯỚC 2: COMPUTE HMAC-SHA256
    /// - Key: Secret key từ settings
    /// - Data: Data string từ bước 1
    /// - Output: 32 bytes (256 bits)
    /// 
    /// BƯỚC 3: CONVERT TO BASE64
    /// - Bytes → Base64 string
    /// - Dễ truyền trong HTTP header
    /// - VÍ DỤ: "abc123def456ghi789..."
    /// 
    /// VÍ DỤ CHI TIẾT:
    /// 
    /// Input:
    /// - method = "POST"
    /// - path = "/api/users"
    /// - timestamp = 1705317600
    /// - body = "{\"email\":\"user@example.com\"}"
    /// - secretKey = "MySecretKey123"
    /// 
    /// Step 1 - Data string:
    /// "POST|/api/users|1705317600|{\"email\":\"user@example.com\"}"
    /// 
    /// Step 2 - HMAC-SHA256:
    /// keyBytes = UTF8("MySecretKey123")
    /// dataBytes = UTF8("POST|/api/users|...")
    /// hash = HMAC-SHA256(dataBytes, keyBytes)
    /// // hash = [0x12, 0x34, 0x56, ...] (32 bytes)
    /// 
    /// Step 3 - Base64:
    /// signature = Base64(hash)
    /// // signature = "EjRWeJCrze8/DQ0NDQ0NDQ0NDQ0NDQ0NDQ0="
    /// 
    /// CÁCH CLIENT DÙNG:
    /// 
    /// // C# Client
    /// var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    /// var body = JsonSerializer.Serialize(request);
    /// var signature = _signatureService.GenerateSignature("POST", "/api/users", timestamp, body);
    /// 
    /// httpClient.DefaultRequestHeaders.Add("X-Timestamp", timestamp.ToString());
    /// httpClient.DefaultRequestHeaders.Add("X-Signature", signature);
    /// 
    /// // JavaScript Client
    /// const timestamp = Math.floor(Date.now() / 1000);
    /// const body = JSON.stringify(data);
    /// const signature = await generateSignature("POST", "/api/users", timestamp, body);
    /// 
    /// fetch("/api/users", {
    ///   method: "POST",
    ///   headers: {
    ///     "X-Timestamp": timestamp,
    ///     "X-Signature": signature
    ///   },
    ///   body: body
    /// });
    /// </summary>
    public string GenerateSignature(string method, string path, long timestamp, string? body = null)
    {
        // BƯỚC 1: TẠO DATA STRING
        // Format: "{method}|{path}|{timestamp}|{body}"
        var data = $"{method}|{path}|{timestamp}|{body ?? string.Empty}";

        // BƯỚC 2: CONVERT TO BYTES
        var keyBytes = Encoding.UTF8.GetBytes(_settings.SecretKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        // BƯỚC 3: COMPUTE HMAC-SHA256
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);

        // BƯỚC 4: CONVERT TO BASE64
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// VALIDATE SIGNATURE TỪ REQUEST
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 
    /// 1. TẠO EXPECTED SIGNATURE:
    ///    - Dùng data từ request (method, path, timestamp, body)
    ///    - Gọi GenerateSignature() với secret key
    ///    - Được signature server tính
    /// 
    /// 2. SO SÁNH VỚI CLIENT SIGNATURE:
    ///    - expectedSignature == clientSignature ?
    ///    - Nếu match → Valid (request không bị sửa)
    ///    - Nếu không match → Invalid (request bị tamper)
    /// 
    /// TẠI SAO AN TOÀN?
    /// 
    /// CASE 1 - ATTACKER SỬA BODY:
    /// Original request:
    ///   Body: {"email":"user@example.com"}
    ///   Signature: abc123...
    /// 
    /// Attacker sửa:
    ///   Body: {"email":"hacker@example.com"}  ← Sửa
    ///   Signature: abc123...  ← Giữ nguyên
    /// 
    /// Server validate:
    ///   expectedSignature = HMAC("POST|..|{\"email\":\"hacker@...\"}")
    ///   expectedSignature != abc123...  → REJECT ✗
    /// 
    /// CASE 2 - ATTACKER TẠO SIGNATURE MỚI:
    /// Attacker không có secret key → Không tạo được signature đúng → REJECT ✗
    /// 
    /// CASE 3 - ATTACKER REPLAY REQUEST CŨ:
    /// Timestamp quá cũ → IsTimestampValid() = false → REJECT ✗
    /// 
    /// VÍ DỤ CHI TIẾT:
    /// 
    /// // Request hợp lệ
    /// POST /api/users
    /// X-Timestamp: 1705317600
    /// X-Signature: abc123def456...
    /// Body: {"email":"user@example.com"}
    /// 
    /// // Server validate
    /// var expectedSig = GenerateSignature("POST", "/api/users", 1705317600, body);
    /// // expectedSig = "abc123def456..."
    /// 
    /// if (expectedSig == "abc123def456...")
    ///     return true;  // ✓ Valid
    /// 
    /// // Request bị sửa
    /// POST /api/users
    /// X-Timestamp: 1705317600
    /// X-Signature: abc123def456...
    /// Body: {"email":"hacker@example.com"}  ← Sửa
    /// 
    /// // Server validate
    /// var expectedSig = GenerateSignature("POST", "/api/users", 1705317600, body);
    /// // expectedSig = "xyz789different..."  ← Khác
    /// 
    /// if (expectedSig == "abc123def456...")
    ///     return false;  // ✗ Invalid
    /// </summary>
    public bool ValidateSignature(string method, string path, long timestamp, string signature, string? body = null)
    {
        // TẠO EXPECTED SIGNATURE từ request data
        var expectedSignature = GenerateSignature(method, path, timestamp, body);

        // SO SÁNH VỚI CLIENT SIGNATURE
        // Nếu match → Valid
        // Nếu không match → Invalid (request bị tamper)
        return expectedSignature == signature;
    }

    /// <summary>
    /// KIỂM TRA TIMESTAMP CÒN VALID KHÔNG
    /// 
    /// MỤC ĐÍCH: Chống REPLAY ATTACK
    /// 
    /// REPLAY ATTACK LÀ GÌ?
    /// - Attacker chặn request hợp lệ
    /// - Gửi lại request đó nhiều lần
    /// - VD: Transfer money request
    /// 
    /// VÍ DỤ REPLAY ATTACK:
    /// 
    /// 10:00 - User gửi request:
    ///   POST /api/transfer
    ///   Body: {"to":"Bob","amount":100}
    ///   Signature: abc123...
    ///   → Success: Transfer $100 to Bob
    /// 
    /// 10:05 - Attacker chặn được request (sniff network)
    /// 
    /// 10:10 - Attacker replay request:
    ///   POST /api/transfer
    ///   Body: {"to":"Bob","amount":100}  ← Giống hệt
    ///   Signature: abc123...  ← Giống hệt
    ///   → Nếu không có timestamp check: Transfer thêm $100 ✗
    /// 
    /// CÁCH PHÒNG CHỐNG:
    /// - Mỗi request có timestamp
    /// - Server check timestamp không quá cũ
    /// - Nếu quá cũ (> expirySeconds) → REJECT
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 
    /// 1. LẤY CURRENT TIMESTAMP:
    ///    currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    ///    VD: 1705317900 (10:05:00)
    /// 
    /// 2. TÍNH DIFFERENCE:
    ///    difference = |currentTimestamp - requestTimestamp|
    ///    VD: |1705317900 - 1705317600| = 300 seconds (5 phút)
    /// 
    /// 3. SO SÁNH VỚI EXPIRY:
    ///    if (difference <= expirySeconds)
    ///        return true;   // Valid (còn trong thời hạn)
    ///    else
    ///        return false;  // Invalid (đã expire)
    /// 
    /// VÍ DỤ CHI TIẾT:
    /// 
    /// ExpirySeconds = 300 (5 phút)
    /// 
    /// CASE 1 - Request mới (VALID):
    /// Request timestamp: 10:00:00 (1705317600)
    /// Current time:      10:02:00 (1705317720)
    /// Difference:        120 seconds
    /// Result:            120 <= 300 → VALID ✓
    /// 
    /// CASE 2 - Request cũ (INVALID):
    /// Request timestamp: 10:00:00 (1705317600)
    /// Current time:      10:07:00 (1705318020)
    /// Difference:        420 seconds
    /// Result:            420 > 300 → INVALID ✗ (Expired)
    /// 
    /// CASE 3 - Request từ tương lai (INVALID):
    /// Request timestamp: 10:10:00 (1705318200)  ← Timestamp tương lai
    /// Current time:      10:00:00 (1705317600)
    /// Difference:        600 seconds
    /// Result:            600 > 300 → INVALID ✗
    /// 
    /// CLOCK SKEW ISSUES:
    /// - Client và Server clock có thể lệch
    /// - Solution: Dùng |difference| (absolute value)
    /// - Allow cả past và future trong expiry window
    /// 
    /// BEST PRACTICES:
    /// - ExpirySeconds = 300 (5 phút): Balanced
    /// - Quá nhỏ (< 60s): Dễ bị clock skew issues
    /// - Quá lớn (> 600s): Dễ bị replay attack
    /// </summary>
    public bool IsTimestampValid(long timestamp, int expirySeconds)
    {
        // LẤY CURRENT TIMESTAMP (server time)
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // TÍNH DIFFERENCE (absolute value để handle cả past và future)
        var difference = Math.Abs(currentTimestamp - timestamp);

        // CHECK TRONG EXPIRY WINDOW
        // difference <= expirySeconds: Valid
        // difference > expirySeconds: Expired
        return difference <= expirySeconds;
    }
}