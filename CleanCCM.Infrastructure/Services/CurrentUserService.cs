using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CleanCCM.Application.Common.Interfaces;

namespace CleanCCM.Infrastructure.Services;

/// <summary>
/// SERVICE LẤY THÔNG TIN USER HIỆN TẠI
/// 
/// SỬ DỤNG:
/// - Lấy UserId từ JWT claims
/// - Lấy Roles từ JWT claims
/// - Dùng trong Handlers để check permissions
/// 
/// VÍ DỤ:
/// public class DeleteUserHandler
/// {
///     private readonly ICurrentUserService _currentUserService;
///     
///     public async Task<Result> Handle(...)
///     {
///         var currentUserId = _currentUserService.UserId;
///         var isAdmin = _currentUserService.Roles.Contains("Admin");
///         
///         if (!isAdmin)
///             return Error.Forbidden(...);
///     }
/// }
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Lấy User ID từ JWT token
    /// 
    /// RETURNS:
    /// - string: User ID nếu authenticated
    /// - null: Nếu chưa login
    /// </summary>
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Lấy Email từ JWT token
    /// 
    /// RETURNS:
    /// - string: Email nếu có
    /// - null: Nếu không có
    /// </summary>
    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    /// <summary>
    /// Lấy Username từ JWT token
    /// 
    /// RETURNS:
    /// - string: Username nếu có
    /// - null: Nếu không có
    /// </summary>
    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Lấy danh sách Roles từ JWT token
    /// 
    /// RETURNS:
    /// - IEnumerable<string>: Danh sách roles
    /// - Empty list nếu không có roles
    /// </summary>
    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
        ?? Enumerable.Empty<string>();

    /// <summary>
    /// Kiểm tra user đã authenticated chưa
    /// 
    /// RETURNS:
    /// - true: Đã login
    /// - false: Chưa login
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}