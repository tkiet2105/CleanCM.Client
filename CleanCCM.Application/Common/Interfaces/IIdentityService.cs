

using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Auth.DTOs;

namespace CleanCCM.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<string>> RegisterAsync(string email, string userName, string password, string? firstName, string? lastName);
    Task<Result<AuthResponse>> LoginAsync(string email, string password);
    Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<Result> RevokeRefreshTokenAsync(string userId);
}