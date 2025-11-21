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

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
        ?? new List<string>();

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool IsInRoles(params string[] roles)
    {
        foreach (var r in roles)
        {
            if (IsInRole(r))
                return true;
        }
        return false;
    }

    public bool IsAdmin => IsInRole("Administrator");
}