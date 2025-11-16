using MediatR;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Auth.DTOs;

namespace CleanCCM.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// COMMAND ĐỂ REFRESH ACCESS TOKEN
/// 
/// GIẢI THÍCH:
/// - Dùng khi access token hết hạn
/// - Gửi expired access token + refresh token
/// - Nhận access token mới + refresh token mới
/// 
/// FLOW:
/// 1. Access token expire (1h)
/// 2. Client gửi RefreshTokenCommand
/// 3. Server validate refresh token
/// 4. Generate new tokens
/// 5. Client lưu và dùng tokens mới
/// 
/// BẢO MẬT:
/// - Refresh token được lưu trong database
/// - Mỗi lần refresh → Generate token mới → Invalidate token cũ
/// - Chống replay attack
/// </summary>
public record RefreshTokenCommand : IRequest<Result<AuthResponse>>
{
    /// <summary>
    /// ACCESS TOKEN (đã expire)
    /// 
    /// GIẢI THÍCH:
    /// - Expired token vẫn chứa claims hợp lệ
    /// - Dùng để extract userId
    /// - Không validate expiry (đã expire rồi)
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// REFRESH TOKEN (còn hạn)
    /// 
    /// GIẢI THÍCH:
    /// - Token để lấy access token mới
    /// - Phải match với token trong database
    /// - Phải còn hạn (< 7 days)
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}