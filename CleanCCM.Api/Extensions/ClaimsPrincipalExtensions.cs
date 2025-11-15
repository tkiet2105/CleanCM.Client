using System.Security.Claims;

namespace CleanCCM.API.Extensions;

/// <summary>
/// EXTENSION METHODS CHO ClaimsPrincipal
/// Giúp lấy thông tin user từ JWT claims dễ dàng hơn
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Lấy User ID từ claims
    /// 
    /// SỬ DỤNG:
    /// var userId = User.GetUserId();
    /// if (userId == null) return Unauthorized();
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Lấy Email từ claims
    /// 
    /// SỬ DỤNG:
    /// var email = User.GetEmail();
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Lấy Username từ claims
    /// 
    /// SỬ DỤNG:
    /// var username = User.GetUserName();
    /// </summary>
    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Lấy danh sách Roles từ claims
    /// 
    /// SỬ DỤNG:
    /// var roles = User.GetRoles();
    /// if (roles.Contains("Admin")) { ... }
    /// </summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }

    /// <summary>
    /// Kiểm tra có role cụ thể không
    /// 
    /// SỬ DỤNG:
    /// if (!User.HasRole("Admin"))
    ///     return Forbid();
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }

    /// <summary>
    /// Kiểm tra có bất kỳ role nào trong danh sách không
    /// 
    /// SỬ DỤNG:
    /// if (!User.HasAnyRole("Admin", "Manager"))
    ///     return Forbid();
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.Any(role => principal.IsInRole(role));
    }

    /// <summary>
    /// Kiểm tra có TẤT CẢ roles trong danh sách không
    /// 
    /// SỬ DỤNG:
    /// if (!User.HasAllRoles("Admin", "SuperUser"))
    ///     return Forbid();
    /// </summary>
    public static bool HasAllRoles(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.All(role => principal.IsInRole(role));
    }
}