using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Auth.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Errors;

namespace CleanCCM.Infrastructure.Identity;



public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> RegisterAsync(
        string email,
        string userName,
        string password,
        string? firstName,
        string? lastName)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return Result<string>.Failure(
                Error.Conflict(AuthErrors.EmailExists, "Email is already in use")
            );
        }

        var existingUserName = await _userManager.FindByNameAsync(userName);
        if (existingUserName != null)
        {
            return Result<string>.Failure(
                Error.Conflict(AuthErrors.UsernameExists, "Username is already in use")
            );
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description
                ?? "User creation failed";

            return Result<string>.Failure(
                Error.Validation(AuthErrors.UserCreationFailed, errorMessage)
            );
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Result<string>.Success(user.Id);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(AuthErrors.InvalidCredentials, "Invalid email or password")
            );
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(AuthErrors.AccountInactive, "Account is inactive")
            );
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Result<AuthResponse>.Failure(
                    Error.Unauthorized(AuthErrors.AccountLocked, "Account is locked due to multiple failed login attempts")
                );
            }

            return Result<AuthResponse>.Failure(
                Error.Unauthorized(AuthErrors.InvalidCredentials, "Invalid email or password")
            );
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email!,
            UserName = user.UserName!
        };

        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        }
        catch
        {
            // ✅ SỬA: Result<AuthResponse>.Failure thay vì Result.Failure
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(BaseErrors.InvalidToken, "Invalid token")
            );
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            // ✅ SỬA: Result<AuthResponse>.Failure
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(BaseErrors.InvalidToken, "Invalid token")
            );
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            // ✅ SỬA: Result<AuthResponse>.Failure
            return Result<AuthResponse>.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "User not found")
            );
        }

        if (user.RefreshToken != refreshToken)
        {
            // ✅ SỬA: Result<AuthResponse>.Failure
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(AuthErrors.InvalidRefreshToken, "Invalid refresh token")
            );
        }

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            // ✅ SỬA: Result<AuthResponse>.Failure
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(BaseErrors.ExpiredToken, "Refresh token has expired")
            );
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var authResponse = new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email!,
            UserName = user.UserName!
        };

        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "User not found")
            );
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}