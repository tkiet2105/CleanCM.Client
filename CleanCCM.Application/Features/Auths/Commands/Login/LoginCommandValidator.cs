using FluentValidation;

namespace CleanCCM.Application.Features.Auth.Commands.Login;

/// <summary>
/// VALIDATOR CHO LoginCommand
/// 
/// GIẢI THÍCH:
/// - Validation đơn giản hơn Register
/// - Chỉ check required và format
/// - KHÔNG check password complexity (vì đang login)
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // EMAIL VALIDATION
        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required")
            .EmailAddress()
                .WithMessage("Invalid email format");

        // PASSWORD VALIDATION
        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required");

        // LƯU Ý:
        // KHÔNG validate password complexity khi login
        // Vì user có thể đã tạo account với password policy cũ
    }
}