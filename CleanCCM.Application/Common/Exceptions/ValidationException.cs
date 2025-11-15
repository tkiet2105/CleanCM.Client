using FluentValidation.Results;

namespace CleanCCM.Application.Common.Exceptions;

/// <summary>
/// EXCEPTION CHO LỖI VALIDATION (từ FluentValidation)
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// VALIDATION EXCEPTION LÀ GÌ?
/// - Exception được throw khi dữ liệu đầu vào KHÔNG HỢP LỆ
/// - Chứa danh sách tất cả validation errors
/// - Được throw bởi ValidationBehaviour (MediatR pipeline)
/// 
/// FLOW HOẠT ĐỘNG:
/// 
/// 1. CLIENT GỬI REQUEST:
///    POST /api/auth/register
///    {
///      "email": "invalid-email",     ← Sai format
///      "password": "123",             ← Quá ngắn
///      "confirmPassword": "456"       ← Không khớp
///    }
/// 
/// 2. REQUEST → MEDIATR PIPELINE:
///    Request → ValidationBehaviour → Handler
///                      ↓
///              (Check validators)
/// 
/// 3. VALIDATORS THỰC THI:
///    - EmailValidator: "Invalid email format"
///    - PasswordValidator: "Password too short"
///    - ConfirmPasswordValidator: "Passwords don't match"
/// 
/// 4. ValidationBehaviour THROW EXCEPTION:
///    throw new ValidationException(validationFailures);
/// 
/// 5. EXCEPTION MIDDLEWARE BẮT VÀ TRẢ VỀ:
///    {
///      "statusCode": 400,
///      "error": {
///        "code": "Validation.Failed",
///        "message": "One or more validation errors occurred",
///        "errors": {
///          "Email": ["Invalid email format"],
///          "Password": ["Password too short"],
///          "ConfirmPassword": ["Passwords don't match"]
///        }
///      }
///    }
/// 
/// SO SÁNH VỚI CÁC EXCEPTION KHÁC:
/// 
/// ValidationException:
/// - Dữ liệu đầu vào sai
/// - HTTP 400 Bad Request
/// - Có thể fix bằng cách sửa input
/// 
/// NotFoundException:
/// - Resource không tồn tại
/// - HTTP 404 Not Found
/// - Không thể fix bằng cách sửa input
/// 
/// DomainException:
/// - Vi phạm business rules
/// - HTTP 422 Unprocessable Entity
/// - Logic nghiệp vụ không cho phép
/// 
/// TẠI SAO KHÔNG DÙNG Result<T>?
/// - ValidationException là để CATCH EARLY
/// - Throw ngay trong pipeline, trước khi vào handler
/// - Handler chỉ nhận request ĐÃ HỢP LỆ
/// - Giảm boilerplate validation code trong handler
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// DICTIONARY chứa validation errors theo FIELD
    /// 
    /// GIẢI THÍCH:
    /// - Key: Tên field (Email, Password, FirstName...)
    /// - Value: Array of error messages cho field đó
    /// 
    /// CẤU TRÚC:
    /// {
    ///   "Email": ["Invalid format", "Already exists"],
    ///   "Password": ["Too short", "Missing uppercase"],
    ///   "ConfirmPassword": ["Does not match Password"]
    /// }
    /// 
    /// VÍ DỤ SỬ DỤNG:
    /// 
    /// try
    /// {
    ///     await _mediator.Send(command);
    /// }
    /// catch (ValidationException ex)
    /// {
    ///     foreach (var error in ex.Errors)
    ///     {
    ///         var fieldName = error.Key;      // "Email"
    ///         var messages = error.Value;     // ["Invalid format"]
    ///         
    ///         Console.WriteLine($"{fieldName}:");
    ///         foreach (var message in messages)
    ///         {
    ///             Console.WriteLine($"  - {message}");
    ///         }
    ///     }
    /// }
    /// 
    /// OUTPUT:
    /// Email:
    ///   - Invalid format
    /// Password:
    ///   - Too short
    ///   - Missing uppercase
    /// 
    /// FRONTEND XỬ LÝ:
    /// 
    /// // React example
    /// catch (error) {
    ///   if (error.response.status === 400) {
    ///     const errors = error.response.data.error.errors;
    ///     
    ///     // Hiển thị error cho từng field
    ///     Object.keys(errors).forEach(field => {
    ///       const messages = errors[field];
    ///       setFieldError(field, messages[0]); // Hiển thị message đầu tiên
    ///     });
    ///   }
    /// }
    /// 
    /// UI KẾT QUẢ:
    /// [Email Input]
    /// ❌ Invalid format
    /// 
    /// [Password Input]
    /// ❌ Too short
    /// 
    /// [Confirm Password Input]
    /// ❌ Does not match Password
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Constructor MẶC ĐỊNH - Tạo exception với message mặc định
    /// 
    /// SỬ DỤNG:
    /// throw new ValidationException();
    /// 
    /// KẾT QUẢ:
    /// - Message: "One or more validation failures have occurred."
    /// - Errors: Empty dictionary
    /// 
    /// LƯU Ý:
    /// - Ít khi dùng constructor này
    /// - Thường dùng constructor có IEnumerable<ValidationFailure>
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Constructor với DANH SÁCH ValidationFailure từ FluentValidation
    /// 
    /// GIẢI THÍCH:
    /// - FluentValidation trả về IEnumerable<ValidationFailure>
    /// - Mỗi ValidationFailure có:
    ///   + PropertyName: Tên field
    ///   + ErrorMessage: Thông báo lỗi
    /// - Constructor này GROUP errors theo PropertyName
    /// 
    /// FLOW:
    /// 
    /// 1. FLUENT VALIDATION THỰC THI:
    ///    var validator = new RegisterCommandValidator();
    ///    var validationResult = validator.Validate(command);
    ///    
    ///    validationResult.Errors = [
    ///      { PropertyName: "Email", ErrorMessage: "Invalid format" },
    ///      { PropertyName: "Email", ErrorMessage: "Already exists" },
    ///      { PropertyName: "Password", ErrorMessage: "Too short" }
    ///    ]
    /// 
    /// 2. ValidationBehaviour KIỂM TRA:
    ///    if (!validationResult.IsValid)
    ///    {
    ///        throw new ValidationException(validationResult.Errors);
    ///    }
    /// 
    /// 3. CONSTRUCTOR GROUP ERRORS:
    ///    Errors = {
    ///      "Email": ["Invalid format", "Already exists"],
    ///      "Password": ["Too short"]
    ///    }
    /// 
    /// VÍ DỤ THỰC TẾ:
    /// 
    /// // FluentValidation validator
    /// public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    /// {
    ///     public RegisterCommandValidator()
    ///     {
    ///         RuleFor(x => x.Email)
    ///             .NotEmpty().WithMessage("Email is required")
    ///             .EmailAddress().WithMessage("Invalid email format");
    ///         
    ///         RuleFor(x => x.Password)
    ///             .NotEmpty().WithMessage("Password is required")
    ///             .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    ///     }
    /// }
    /// 
    /// // ValidationBehaviour
    /// var validationResults = await Task.WhenAll(
    ///     _validators.Select(v => v.ValidateAsync(context, cancellationToken))
    /// );
    /// 
    /// var failures = validationResults
    ///     .Where(r => r.Errors.Any())
    ///     .SelectMany(r => r.Errors)
    ///     .ToList();
    /// 
    /// if (failures.Any())
    /// {
    ///     // THROW với danh sách failures
    ///     throw new ValidationException(failures);
    /// }
    /// 
    /// LOGIC GROUPING:
    /// 
    /// Input:
    /// [
    ///   { PropertyName: "Email", ErrorMessage: "Required" },
    ///   { PropertyName: "Email", ErrorMessage: "Invalid" },
    ///   { PropertyName: "Password", ErrorMessage: "Too short" }
    /// ]
    /// 
    /// GroupBy(PropertyName):
    /// {
    ///   "Email": [
    ///     { ErrorMessage: "Required" },
    ///     { ErrorMessage: "Invalid" }
    ///   ],
    ///   "Password": [
    ///     { ErrorMessage: "Too short" }
    ///   ]
    /// }
    /// 
    /// ToDictionary:
    /// {
    ///   "Email": ["Required", "Invalid"],
    ///   "Password": ["Too short"]
    /// }
    /// </summary>
    /// <param name="failures">Danh sách ValidationFailure từ FluentValidation</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        // GROUP errors theo PropertyName
        // Mỗi PropertyName có thể có nhiều error messages
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)  // Group theo field name
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

        // KẾT QUẢ:
        // {
        //   "Email": ["Invalid format", "Already exists"],
        //   "Password": ["Too short", "Missing uppercase"],
        //   "ConfirmPassword": ["Does not match"]
        // }
    }
}