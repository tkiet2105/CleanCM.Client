using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanCCM.Application.Features.Auth.Commands.Register;
using CleanCCM.Application.Features.Auth.Commands.Login;
using CleanCCM.Application.Features.Auth.Commands.RefreshToken;
using CleanCCM.Application.Features.Products.Queries;

namespace CleanCCM.API.Controllers;

/// <summary>
/// AUTH CONTROLLER - Xử lý authentication endpoints
/// 
/// ENDPOINTS:
/// - POST /api/auth/register   → Đăng ký user mới
/// - POST /api/auth/login      → Đăng nhập
/// - POST /api/auth/refresh    → Refresh access token
/// - POST /api/auth/logout     → Logout (revoke refresh token)
/// 
/// ATTRIBUTES:
/// - [AllowAnonymous]: Cho phép access mà không cần authentication
/// - [Authorize]: Yêu cầu authentication
/// </summary>
public class ProductController : BaseApiController
{
    [HttpPost("get-all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(GetAllProductQuery queries)
    {
        var result = await Mediator.Send(queries);

        if (!result.IsSuccess)
            return HandleFailure(result);

        return Ok(result);
    }

   
}