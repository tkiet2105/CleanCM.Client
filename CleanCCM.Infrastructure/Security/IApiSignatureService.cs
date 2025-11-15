namespace CleanCCM.Infrastructure.Security;

/// <summary>
/// SERVICE INTERFACE CHO API SIGNATURE
/// 
/// GIẢI THÍCH:
/// - Interface định nghĩa signature operations
/// - Generate signature (client-side)
/// - Validate signature (server-side)
/// - Check timestamp validity
/// </summary>
public interface IApiSignatureService
{
    /// <summary>
    /// TẠO SIGNATURE CHO REQUEST
    /// 
    /// PARAMETERS:
    /// - method: HTTP method (GET, POST, PUT, DELETE...)
    /// - path: Request path (/api/users, /api/products/123...)
    /// - timestamp: Unix timestamp (seconds since epoch)
    /// - body: Request body (JSON string, null nếu không có body)
    /// 
    /// RETURNS:
    /// - Base64-encoded signature string
    /// 
    /// VÍ DỤ:
    /// var signature = GenerateSignature(
    ///     method: "POST",
    ///     path: "/api/users",
    ///     timestamp: 1705317600,
    ///     body: "{\"email\":\"user@example.com\"}"
    /// );
    /// // signature = "abc123def456..." (Base64)
    /// </summary>
    string GenerateSignature(string method, string path, long timestamp, string? body = null);

    /// <summary>
    /// VALIDATE SIGNATURE TỪ REQUEST
    /// 
    /// PARAMETERS:
    /// - method: HTTP method từ request
    /// - path: Request path
    /// - timestamp: Timestamp từ X-Timestamp header
    /// - signature: Signature từ X-Signature header
    /// - body: Request body
    /// 
    /// RETURNS:
    /// - true: Signature hợp lệ
    /// - false: Signature không hợp lệ (reject request)
    /// 
    /// VÍ DỤ:
    /// var isValid = ValidateSignature(
    ///     method: context.Request.Method,
    ///     path: context.Request.Path,
    ///     timestamp: timestampFromHeader,
    ///     signature: signatureFromHeader,
    ///     body: requestBody
    /// );
    /// 
    /// if (!isValid)
    ///     return Unauthorized("Invalid signature");
    /// </summary>
    bool ValidateSignature(string method, string path, long timestamp, string signature, string? body = null);

    /// <summary>
    /// KIỂM TRA TIMESTAMP CÒN VALID KHÔNG
    /// 
    /// PARAMETERS:
    /// - timestamp: Unix timestamp từ request
    /// - expirySeconds: Số giây cho phép (từ settings)
    /// 
    /// RETURNS:
    /// - true: Timestamp còn valid
    /// - false: Timestamp đã expire (quá cũ)
    /// 
    /// VÍ DỤ:
    /// var isValid = IsTimestampValid(1705317600, 300);
    /// // Check: |currentTime - timestamp| <= 300s
    /// </summary>
    bool IsTimestampValid(long timestamp, int expirySeconds);
}