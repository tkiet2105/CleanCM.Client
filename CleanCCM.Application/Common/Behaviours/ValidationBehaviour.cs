using FluentValidation;
using MediatR;

namespace CleanCCM.Application.Common.Behaviours;

/// <summary>
/// VALIDATION BEHAVIOUR - MediatR Pipeline Behaviour cho validation
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// PIPELINE BEHAVIOUR LÀ GÌ?
/// - Middleware trong MediatR pipeline
/// - Chạy TRƯỚC Handler (pre-processing)
/// - Có thể chạy SAU Handler (post-processing)
/// - Giống middleware trong ASP.NET Core
/// 
/// MEDIATR PIPELINE FLOW:
/// 
/// Request
///   ↓
/// ValidationBehaviour ← Validate request
///   ↓
/// PerformanceBehaviour ← Log performance
///   ↓
/// AuthorizationBehaviour ← Check permissions
///   ↓
/// Handler ← Xử lý business logic
///   ↓
/// Response
/// 
/// TẠI SAO CẦN ValidationBehaviour?
/// 
/// KHÔNG DÙNG BEHAVIOUR (XẤU):
/// 
/// public class CreateUserCommandHandler
/// {
///     public async Task<Result> Handle(CreateUserCommand request)
///     {
///         // Validation code trong EVERY handler
///         if (string.IsNullOrEmpty(request.Email))
///             return Error.Validation(...);
///         
///         if (!IsValidEmail(request.Email))
///             return Error.Validation(...);
///         
///         if (string.IsNullOrEmpty(request.Password))
///             return Error.Validation(...);
///         
///         // Business logic
///         var user = User.Create(...);
///         ...
///     }
/// }
/// 
/// VẤN ĐỀ:
/// - Code duplicate trong mọi handler
/// - Khó maintain
/// - Dễ quên validate
/// - Handler dài dòng
/// 
/// DÙNG BEHAVIOUR (TỐT):
/// 
/// // Validator - Định nghĩa rules
/// public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
/// {
///     public CreateUserCommandValidator()
///     {
///         RuleFor(x => x.Email).NotEmpty().EmailAddress();
///         RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
///     }
/// }
/// 
/// // Handler - Chỉ xử lý business logic
/// public class CreateUserCommandHandler
/// {
///     public async Task<Result> Handle(CreateUserCommand request)
///     {
///         // Request đã được validate, không cần check nữa!
///         var user = User.Create(request.Email, request.Password);
///         ...
///     }
/// }
/// 
/// ƯU ĐIỂM:
/// - Tách biệt validation và business logic
/// - Validator tái sử dụng được
/// - Handler clean, tập trung vào logic
/// - Tự động validate tất cả requests
/// 
/// FLUENT VALIDATION LÀ GÌ?
/// - Library mạnh mẽ cho validation
/// - Fluent API (method chaining)
/// - Built-in validators (Email, Length, Range...)
/// - Custom validators
/// - Async validation
/// 
/// VÍ DỤ VALIDATOR:
/// 
/// public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
/// {
///     public RegisterCommandValidator()
///     {
///         RuleFor(x => x.Email)
///             .NotEmpty().WithMessage("Email is required")
///             .EmailAddress().WithMessage("Invalid email format")
///             .MaximumLength(100).WithMessage("Email too long");
///         
///         RuleFor(x => x.Password)
///             .NotEmpty().WithMessage("Password is required")
///             .MinimumLength(8).WithMessage("Password must be at least 8 characters")
///             .Matches(@"[A-Z]").WithMessage("Must contain uppercase")
///             .Matches(@"[a-z]").WithMessage("Must contain lowercase")
///             .Matches(@"[0-9]").WithMessage("Must contain number");
///         
///         RuleFor(x => x.ConfirmPassword)
///             .Equal(x => x.Password).WithMessage("Passwords must match");
///     }
/// }
/// </summary>
/// <typeparam name="TRequest">Type của request (Command hoặc Query)</typeparam>
/// <typeparam name="TResponse">Type của response (Result, Result<T>...)</typeparam>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// DANH SÁCH VALIDATORS cho request type này
    /// 
    /// GIẢI THÍCH:
    /// - Dependency Injection tự động inject validators
    /// - Một request có thể có NHIỀU validators
    /// - Tất cả validators sẽ được chạy
    /// 
    /// VÍ DỤ:
    /// // RegisterCommand có 1 validator
    /// IEnumerable<IValidator<RegisterCommand>> = [
    ///     RegisterCommandValidator
    /// ]
    /// 
    /// // Nếu không có validator
    /// IEnumerable<IValidator<SomeCommand>> = []  // Empty
    /// 
    /// DEPENDENCY INJECTION:
    /// services.AddValidatorsFromAssembly(assembly);
    /// → Tự động scan và register tất cả validators
    /// 
    /// NAMING CONVENTION:
    /// - Command: CreateUserCommand
    /// - Validator: CreateUserCommandValidator
    /// → DI tự động match
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Constructor - Inject validators
    /// 
    /// GIẢI THÍCH:
    /// - MediatR tự động inject validators phù hợp
    /// - Nếu không có validator → _validators = empty collection
    /// 
    /// VÍ DỤ:
    /// // RegisterCommand
    /// new ValidationBehaviour<RegisterCommand, Result>(
    ///     validators: [RegisterCommandValidator]
    /// )
    /// 
    /// // GetUserQuery (không có validator)
    /// new ValidationBehaviour<GetUserQuery, Result<User>>(
    ///     validators: []  // Empty
    /// )
    /// </summary>
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// HANDLE METHOD - Logic chính của behaviour
    /// 
    /// GIẢI THÍCH:
    /// - Method được gọi bởi MediatR pipeline
    /// - Chạy TRƯỚC handler
    /// - Có thể short-circuit (không gọi next)
    /// 
    /// FLOW:
    /// 
    /// 1. KIỂM TRA CÓ VALIDATORS KHÔNG:
    ///    if (!_validators.Any()) → Skip validation, gọi next()
    /// 
    /// 2. TẠO VALIDATION CONTEXT:
    ///    var context = new ValidationContext<TRequest>(request);
    /// 
    /// 3. CHẠY TẤT CẢ VALIDATORS:
    ///    var results = await Task.WhenAll(
    ///        _validators.Select(v => v.ValidateAsync(context))
    ///    );
    /// 
    /// 4. THU THẬP LỖI:
    ///    var failures = results
    ///        .Where(r => r.Errors.Any())
    ///        .SelectMany(r => r.Errors);
    /// 
    /// 5. NẾU CÓ LỖI:
    ///    throw new ValidationException(failures);
    /// 
    /// 6. NẾU KHÔNG CÓ LỖI:
    ///    return await next(); → Gọi handler tiếp theo
    /// 
    /// VÍ DỤ THỰC TẾ:
    /// 
    /// // Request
    /// var command = new RegisterCommand
    /// {
    ///     Email = "invalid-email",
    ///     Password = "123"
    /// };
    /// 
    /// // MediatR.Send()
    /// await _mediator.Send(command);
    /// 
    /// // ValidationBehaviour.Handle()
    /// → Validators chạy:
    ///   - EmailValidator: FAIL ("Invalid email")
    ///   - PasswordValidator: FAIL ("Too short")
    /// 
    /// → Throw ValidationException với 2 errors
    /// 
    /// → Handler KHÔNG được gọi (short-circuit)
    /// 
    /// → ExceptionMiddleware bắt và trả 400 Bad Request
    /// 
    /// PERFORMANCE:
    /// - Task.WhenAll: Chạy song song tất cả validators
    /// - Nhanh hơn chạy tuần tự
    /// 
    /// VÍ DỤ:
    /// // 3 validators, mỗi cái 100ms
    /// // Tuần tự: 300ms
    /// // Song song (WhenAll): ~100ms
    /// </summary>
    /// <param name="request">Request object (Command/Query)</param>
    /// <param name="next">Delegate gọi handler tiếp theo trong pipeline</param>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <returns>Response từ handler (hoặc throw ValidationException)</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // BƯỚC 1: Kiểm tra có validators không
        if (!_validators.Any())
        {
            // Không có validators → Skip validation
            // Gọi handler tiếp theo trong pipeline
            return await next();
        }

        // BƯỚC 2: Tạo validation context
        // Context chứa request và metadata cho validators
        var context = new ValidationContext<TRequest>(request);

        // BƯỚC 3: Chạy TẤT CẢ validators SONG SONG
        // Task.WhenAll: Chờ tất cả validators hoàn thành
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        // VÍ DỤ validationResults:
        // [
        //   { IsValid: false, Errors: [{ PropertyName: "Email", ErrorMessage: "Invalid" }] },
        //   { IsValid: false, Errors: [{ PropertyName: "Password", ErrorMessage: "Too short" }] }
        // ]

        // BƯỚC 4: THU THẬP TẤT CẢ LỖI
        var failures = validationResults
            .Where(r => r.Errors.Any())           // Lọc results có lỗi
            .SelectMany(r => r.Errors)             // Flatten errors thành 1 list
            .ToList();

        // VÍ DỤ failures:
        // [
        //   { PropertyName: "Email", ErrorMessage: "Invalid format" },
        //   { PropertyName: "Password", ErrorMessage: "Too short" },
        //   { PropertyName: "ConfirmPassword", ErrorMessage: "Must match" }
        // ]

        // BƯỚC 5: NẾU CÓ LỖI → THROW EXCEPTION
        if (failures.Any())
        {
            // Throw ValidationException
            // → ExceptionMiddleware sẽ bắt và trả 400 Bad Request
            // → Handler KHÔNG được gọi
            throw new Exceptions.ValidationException(failures);
        }

        // BƯỚC 6: KHÔNG CÓ LỖI → Gọi handler tiếp theo
        // Request đã hợp lệ, tiếp tục pipeline
        return await next();
    }
}