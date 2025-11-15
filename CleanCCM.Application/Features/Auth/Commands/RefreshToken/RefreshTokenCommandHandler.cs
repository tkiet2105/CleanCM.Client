using MediatR;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Auth.DTOs;
using CleanCCM.Application.Common.Interfaces;

namespace CleanCCM.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// HANDLER XỬ LÝ RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// XỬ LÝ REFRESH TOKEN
    /// 
    /// FLOW:
    /// 1. Extract userId from expired access token
    /// 2. Find user in database
    /// 3. Validate refresh token
    /// 4. Generate new tokens
    /// 5. Update user.RefreshToken in database
    /// 6. Return new AuthResponse
    /// </summary>
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken
        );

        return result;
    }
}