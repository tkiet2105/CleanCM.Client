using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Errors.Auth;
using CleanCCM.Domain.Common.Errors;
using System.Text.Json;

namespace CleanCCM.Infrastructure.Security;

/// <summary>
/// MIDDLEWARE VALIDATE API SIGNATURE
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// MIDDLEWARE LÀ GÌ?
/// - Component trong ASP.NET Core pipeline
/// - Chạy TRƯỚC controller
/// - Có thể short-circuit (không gọi next)
/// 
/// MIDDLEWARE NÀY LÀM GÌ?
/// - Validate API signature cho mọi request
/// - Reject requests không có signature hoặc signature sai
/// - Skip validation cho excluded paths
/// 
/// FLOW:
/// 
/// Request
///   ↓
/// SignatureValidationMiddleware
///   ↓ (check signature)
///   ├─ Valid → next() → Controller
///   └─ Invalid → 401 Response (không gọi controller)
/// 
/// VÍ DỤ REQUEST:
/// 
/// POST /api/users
/// Headers:
///   X-Timestamp: 1705317600
///   X-Signature: abc123def456...
/// Body: {"email":"user@example.com"}
/// 
/// VALIDATION STEPS:
/// 1. Check middleware enabled?
/// 2. Check path excluded?
/// 3. Check X-Timestamp header exists?
/// 4. Check timestamp valid (not expired)?
/// 5. Check X-Signature header exists?
/// 6. Validate signature match?
/// 
/// Nếu ALL PASS → Next (controller)
/// Nếu ANY FAIL → 401 Error
/// </summary>
public class SignatureValidationMiddleware
{
    /// <summary>
    /// NEXT MIDDLEWARE trong pipeline
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// SETTINGS cho signature validation
    /// </summary>
    private readonly ApiSignatureSettings _settings;

    /// <summary>
    /// Constructor - Inject dependencies
    /// 
    /// LƯU Ý:
    /// - _next: Injected by framework (singleton)
    /// - _settings: Injected by framework (singleton)
    /// - IApiSignatureService: Injected per request (scoped) trong InvokeAsync
    /// </summary>
    public SignatureValidationMiddleware(
        RequestDelegate next,
        IOptions<ApiSignatureSettings> settings)
    {
        _next = next;
        _settings = settings.Value;
    }

    /// <summary>
    /// INVOKE METHOD - Logic chính của middleware
    /// 
    /// FLOW CHI TIẾT:
    /// 
    /// === BƯỚC 1: CHECK ENABLED ===
    /// if (!_settings.Enabled)
    ///     return next();  // Skip validation
    /// 
    /// === BƯỚC 2: CHECK EXCLUDED PATHS ===
    /// if (path in ExcludedPaths)
    ///     return next();  // Skip validation
    /// 
    /// === BƯỚC 3: VALIDATE TIMESTAMP HEADER ===
    /// if (no X-Timestamp header)
    ///     return 401 (MissingTimestamp)
    /// 
    /// if (timestamp invalid format)
    ///     return 401 (MissingTimestamp)
    /// 
    /// if (timestamp expired)
    ///     return 401 (ExpiredSignature)
    /// 
    /// === BƯỚC 4: VALIDATE SIGNATURE HEADER ===
    /// if (no X-Signature header)
    ///     return 401 (MissingSignature)
    /// 
    /// === BƯỚC 5: READ BODY ===
    /// body = read request body
    /// 
    /// === BƯỚC 6: VALIDATE SIGNATURE ===
    /// if (!ValidateSignature(...))
    ///     return 401 (InvalidSignature)
    /// 
    /// === BƯỚC 7: PASS TO NEXT ===
    /// return next();  // Call controller
    /// 
    /// VÍ DỤ REQUEST HỢP LỆ:
    /// 
    /// POST /api/users
    /// X-Timestamp: 1705317600
    /// X-Signature: abc123def456...
    /// Body: {"email":"user@example.com"}
    /// 
    /// → Enabled = true
    /// → Path not excluded
    /// → Timestamp exists and valid
    /// → Signature exists and valid
    /// → next() → Controller
    /// 
    /// VÍ DỤ REQUEST KHÔNG HỢP LỆ:
    /// 
    /// POST /api/users
    /// X-Timestamp: 1705317000  ← Quá cũ (10 phút trước)
    /// X-Signature: abc123def456...
    /// Body: {"email":"user@example.com"}
    /// 
    /// → Enabled = true
    /// → Path not excluded
    /// → Timestamp exists but EXPIRED
    /// → Return 401 (ExpiredSignature)
    /// → Controller KHÔNG được gọi
    /// </summary>
    /// <param name="context">HttpContext của request</param>
    /// <param name="signatureService">Service validate signature (scoped per request)</param>
    public async Task InvokeAsync(HttpContext context, IApiSignatureService signatureService)
    {
        // ========== BƯỚC 1: CHECK ENABLED ==========

        /// <summary>
        /// Nếu signature validation TẮT → Skip
        /// 
        /// KHI NÀO TẮT?
        /// - Development/Testing
        /// - Internal APIs không cần signature
        /// </summary>
        if (!_settings.Enabled)
        {
            await _next(context);
            return;
        }

        // ========== BƯỚC 2: CHECK EXCLUDED PATHS ==========

        /// <summary>
        /// Nếu path trong excluded list → Skip
        /// 
        /// VÍ DỤ:
        /// ExcludedPaths = ["/api/auth/login", "/api/health"]
        /// 
        /// Request: POST /api/auth/login
        /// → Skip validation (login không cần signature)
        /// </summary>
        var path = context.Request.Path.Value ?? string.Empty;
        if (_settings.ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // ========== BƯỚC 3: VALIDATE TIMESTAMP HEADER ==========

        /// <summary>
        /// CHECK X-Timestamp HEADER
        /// 
        /// HEADER REQUIRED:
        /// X-Timestamp: 1705317600
        /// 
        /// FORMAT:
        /// - Unix timestamp (seconds since epoch)
        /// - Long integer (Int64)
        /// 
        /// VÍ DỤ:
        /// X-Timestamp: 1705317600  → Valid
        /// X-Timestamp: invalid     → Invalid format
        /// (no header)              → Missing
        /// </summary>
        if (!context.Request.Headers.TryGetValue("X-Timestamp", out var timestampHeader))
        {
            await WriteErrorResponse(context, AuthErrors.MissingTimestamp, "Timestamp is required");
            return;
        }

        if (!long.TryParse(timestampHeader, out var timestamp))
        {
            await WriteErrorResponse(context, AuthErrors.MissingTimestamp, "Invalid timestamp format");
            return;
        }

        /// <summary>
        /// CHECK TIMESTAMP EXPIRY
        /// 
        /// LOGIC:
        /// |currentTime - timestamp| <= ExpirySeconds
        /// 
        /// VÍ DỤ:
        /// Timestamp: 1705317600 (10:00:00)
        /// Current:   1705317720 (10:02:00)
        /// Expiry:    300 seconds (5 minutes)
        /// Diff:      120 seconds
        /// Result:    120 <= 300 → VALID ✓
        /// 
        /// Timestamp: 1705317600 (10:00:00)
        /// Current:   1705318020 (10:07:00)
        /// Expiry:    300 seconds (5 minutes)
        /// Diff:      420 seconds
        /// Result:    420 > 300 → EXPIRED ✗
        /// </summary>
        if (!signatureService.IsTimestampValid(timestamp, _settings.ExpirySeconds))
        {
            await WriteErrorResponse(context, AuthErrors.ExpiredSignature, "Signature has expired");
            return;
        }

        // ========== BƯỚC 4: VALIDATE SIGNATURE HEADER ==========

        /// <summary>
        /// CHECK X-Signature HEADER
        /// 
        /// HEADER REQUIRED:
        /// X-Signature: abc123def456...
        /// 
        /// FORMAT:
        /// - Base64-encoded string
        /// - HMAC-SHA256 hash
        /// 
        /// VÍ DỤ:
        /// X-Signature: abc123def456...  → Valid format
        /// (no header)                   → Missing
        /// </summary>
        if (!context.Request.Headers.TryGetValue("X-Signature", out var signature))
        {
            await WriteErrorResponse(context, AuthErrors.MissingSignature, "Signature is required");
            return;
        }

        // ========== BƯỚC 5: READ BODY ==========

        /// <summary>
        /// ĐỌC REQUEST BODY
        /// 
        /// LƯU Ý:
        /// - EnableBuffering(): Cho phép đọc body nhiều lần
        /// - Nếu không enable → body chỉ đọc được 1 lần
        /// - Position = 0: Reset stream để controller có thể đọc lại
        /// 
        /// VÍ DỤ:
        /// POST /api/users
        /// Body: {"email":"user@example.com"}
        /// 
        /// → Read body here (middleware)
        /// → Reset position = 0
        /// → Controller có thể đọc lại body
        /// </summary>
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        // ========== BƯỚC 6: VALIDATE SIGNATURE ==========

        /// <summary>
        /// VALIDATE SIGNATURE
        /// 
        /// INPUTS:
        /// - method: HTTP method (GET, POST, PUT...)
        /// - path: Request path (/api/users)
        /// - timestamp: Timestamp từ header
        /// - signature: Signature từ header
        /// - body: Request body
        /// 
        /// LOGIC:
        /// 1. Server tính expected signature từ request data
        /// 2. So sánh với signature từ client
        /// 3. Match → Valid | Not match → Invalid
        /// 
        /// VÍ DỤ:
        /// Client signature: abc123...
        /// Server signature: abc123... (calculated)
        /// Result: Match → VALID ✓
        /// 
        /// Client signature: abc123... (original)
        /// Body sửa:        {"email":"hacker@..."}
        /// Server signature: xyz789... (calculated from modified body)
        /// Result: Not match → INVALID ✗
        /// </summary>
        var method = context.Request.Method;
        var isValid = signatureService.ValidateSignature(method, path, timestamp, signature!, body);

        if (!isValid)
        {
            await WriteErrorResponse(context, AuthErrors.InvalidSignature, "Invalid signature");
            return;
        }

        // ========== BƯỚC 7: PASS TO NEXT MIDDLEWARE ==========

        /// <summary>
        /// TẤT CẢ VALIDATION PASS → Gọi next middleware/controller
        /// </summary>
        await _next(context);
    }

    /// <summary>
    /// WRITE ERROR RESPONSE
    /// 
    /// GIẢI THÍCH:
    /// - Helper method để trả về error response
    /// - Format: JSON với error object
    /// - Status code: 401 Unauthorized
    /// 
    /// RESPONSE FORMAT:
    /// {
    ///   "error": {
    ///     "code": "AUTH_7011",
    ///     "message": "Signature is required",
    ///     "type": "Unauthorized"
    ///   }
    /// }
    /// 
    /// VÍ DỤ USAGE:
    /// await WriteErrorResponse(context, AuthErrors.MissingSignature, "Signature is required");
    /// </summary>
    private static async Task WriteErrorResponse(HttpContext context, ErrorCode errorCode, string message)
    {
        // SET STATUS CODE
        context.Response.StatusCode = 401;

        // SET CONTENT TYPE
        context.Response.ContentType = "application/json";

        // CREATE ERROR OBJECT
        var error = Error.Unauthorized(errorCode, message);
        var response = new { error };

        // WRITE JSON RESPONSE
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}