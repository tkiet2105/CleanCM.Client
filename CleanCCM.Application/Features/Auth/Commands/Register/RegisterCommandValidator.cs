using FluentValidation;

namespace CleanCCM.Application.Features.Auth.Commands.Register;

/// <summary>
/// VALIDATOR CHO RegisterCommand
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// FLUENT VALIDATION LÀ GÌ?
/// - Library mạnh để validate objects
/// - Fluent API (method chaining)
/// - Tách biệt validation logic khỏi domain/handler
/// 
/// CÁCH HOẠT ĐỘNG:
/// 
/// 1. ĐỊNH NGHĨA RULES:
///    RuleFor(x => x.Email).NotEmpty().EmailAddress();
/// 
/// 2. ValidationBehaviour TỰ ĐỘNG CHẠY:
///    var result = validator.Validate(command);
/// 
/// 3. NẾU FAIL:
///    throw new ValidationException(result.Errors);
/// 
/// 4. NẾU PASS:
///    Tiếp tục đến handler
/// 
/// BUILT-IN VALIDATORS:
/// - NotEmpty(): Không được rỗng
/// - EmailAddress(): Phải là email hợp lệ
/// - MinimumLength(n): Độ dài tối thiểu
/// - MaximumLength(n): Độ dài tối đa
/// - Matches(regex): Phải match regex
/// - Equal(value): Phải bằng giá trị
/// - Must(predicate): Custom validation
/// 
/// VÍ DỤ VALIDATION ERRORS:
/// 
/// Input:
/// {
///   "email": "",
///   "password": "123"
/// }
/// 
/// Output:
/// {
///   "Email": ["Email is required", "Invalid email format"],
///   "Password": ["Password must be at least 8 characters"]
/// }
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Constructor - Định nghĩa validation rules
    /// 
    /// GIẢI THÍCH:
    /// - Rules được define trong constructor
    /// - Execute khi ValidationBehaviour chạy
    /// </summary>
    public RegisterCommandValidator()
    {
        // ========== EMAIL VALIDATION ==========

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is required")
            .EmailAddress()
                .WithMessage("Invalid email format")
            .MaximumLength(100)
                .WithMessage("Email must not exceed 100 characters");

        // GIẢI THÍCH:
        // .NotEmpty(): Email không được rỗng
        // .EmailAddress(): Phải đúng format email (có @, domain...)
        // .MaximumLength(100): Không quá 100 ký tự
        // .WithMessage(): Custom error message

        // VÍ DỤ:
        // Email = "" → "Email is required"
        // Email = "invalid" → "Invalid email format"
        // Email = "very-long-email@..." (>100 chars) → "Email must not exceed 100 characters"

        // ========== USERNAME VALIDATION ==========

        RuleFor(x => x.UserName)
            .NotEmpty()
                .WithMessage("Username is required")
            .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters")
            .MaximumLength(50)
                .WithMessage("Username must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$")
                .WithMessage("Username can only contain letters, numbers, and underscores");

        // GIẢI THÍCH:
        // .MinimumLength(3): Tối thiểu 3 ký tự
        // .MaximumLength(50): Tối đa 50 ký tự
        // .Matches(regex): Chỉ cho phép chữ cái, số, underscore

        // REGEX: ^[a-zA-Z0-9_]+$
        // ^: Bắt đầu string
        // [a-zA-Z0-9_]+: Một hoặc nhiều ký tự (chữ, số, underscore)
        // $: Kết thúc string

        // VÍ DỤ:
        // UserName = "john_doe" ✓
        // UserName = "jd" ✗ (< 3 chars)
        // UserName = "john doe" ✗ (có space)
        // UserName = "john-doe" ✗ (có dấu gạch ngang)

        // ========== PASSWORD VALIDATION ==========

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character");

        // GIẢI THÍCH:
        // .MinimumLength(8): Tối thiểu 8 ký tự
        // .Matches(@"[A-Z]"): Phải có ít nhất 1 chữ HOA
        // .Matches(@"[a-z]"): Phải có ít nhất 1 chữ thường
        // .Matches(@"[0-9]"): Phải có ít nhất 1 số
        // .Matches(@"[^a-zA-Z0-9]"): Phải có ít nhất 1 ký tự đặc biệt

        // REGEX BREAKDOWN:
        // [A-Z]: Bất kỳ chữ HOA nào
        // [a-z]: Bất kỳ chữ thường nào
        // [0-9]: Bất kỳ số nào
        // [^a-zA-Z0-9]: Bất kỳ ký tự KHÔNG PHẢI chữ/số (special char)

        // VÍ DỤ:
        // Password = "SecurePass123!" ✓ (có đủ: Hoa, thường, số, special)
        // Password = "password" ✗ (thiếu Hoa, số, special)
        // Password = "PASSWORD123" ✗ (thiếu thường, special)
        // Password = "Pass123" ✗ (thiếu special, < 8 chars)

        // ========== CONFIRM PASSWORD VALIDATION ==========

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
                .WithMessage("Passwords do not match");

        // GIẢI THÍCH:
        // .Equal(x => x.Password): ConfirmPassword phải BẰNG Password

        // VÍ DỤ:
        // Password = "SecurePass123!"
        // ConfirmPassword = "SecurePass123!" ✓
        // ConfirmPassword = "DifferentPass" ✗ "Passwords do not match"

        // ========== FIRSTNAME VALIDATION (OPTIONAL) ==========

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        // GIẢI THÍCH:
        // .When(): Chỉ validate KHI điều kiện = true
        // Nếu FirstName = null/empty → SKIP validation này
        // Nếu FirstName có giá trị → Check MaxLength

        // VÍ DỤ:
        // FirstName = null ✓ (optional, skip validation)
        // FirstName = "" ✓ (optional, skip validation)
        // FirstName = "John" ✓ (< 100 chars)
        // FirstName = "Very long name..." (>100) ✗

        // ========== LASTNAME VALIDATION (OPTIONAL) ==========

        RuleFor(x => x.LastName)
            .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        // Tương tự FirstName
    }
}