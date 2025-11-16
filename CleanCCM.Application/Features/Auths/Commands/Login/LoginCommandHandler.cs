using MediatR;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Auth.DTOs;
using CleanCCM.Application.Common.Interfaces;

namespace CleanCCM.Application.Features.Auth.Commands.Login;

/// <summary>
/// HANDLER XỬ LÝ LoginCommand
/// 
/// FLOW:
/// 1. Nhận email + password
/// 2. Gọi IdentityService.LoginAsync()
/// 3. Service check credentials
/// 4. Generate JWT tokens
/// 5. Trả về AuthResponse
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// XỬ LÝ LOGIN
    /// 
    /// VÍ DỤ SUCCESS:
    /// 
    /// Input:
    /// {
    ///   "email": "user@example.com",
    ///   "password": "SecurePass123!"
    /// }
    /// 
    /// Output:
    /// Result.IsSuccess = true
    /// Result.Value = {
    ///   accessToken: "eyJhbGc...",
    ///   refreshToken: "xyz123...",
    ///   expiresIn: 3600,
    ///   userId: "3fa85f64...",
    ///   email: "user@example.com",
    ///   userName: "john_doe"
    /// }
    /// 
    /// VÍ DỤ FAILURE (Wrong password):
    /// 
    /// Result.IsSuccess = false
    /// Result.Error = {
    ///   Code: "AUTH_7001",
    ///   Message: "Invalid email or password",
    ///   Type: ErrorType.Unauthorized
    /// }
    /// 
    /// HTTP Response: 401 Unauthorized
    /// </summary>
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(request.Email, request.Password);
        return result;
    }
}