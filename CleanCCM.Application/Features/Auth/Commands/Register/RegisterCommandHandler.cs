using MediatR;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Common.Interfaces;

namespace CleanCCM.Application.Features.Auth.Commands.Register;

/// <summary>
/// HANDLER XỬ LÝ RegisterCommand
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// HANDLER LÀ GÌ?
/// - Class xử lý business logic cho Command/Query
/// - Nhận Request → Xử lý → Trả về Response
/// - Mỗi Command/Query có 1 Handler tương ứng
/// 
/// MEDIATR CONVENTION:
/// - Command: RegisterCommand
/// - Handler: RegisterCommandHandler
/// - Response: Result<string> (userId)
/// 
/// HANDLER LIFECYCLE:
/// 
/// 1. CLIENT GỬI REQUEST:
///    POST /api/auth/register { email, password... }
/// 
/// 2. CONTROLLER TẠO COMMAND:
///    var command = new RegisterCommand { ... };
///    var result = await _mediator.Send(command);
/// 
/// 3. MEDIATR ROUTING:
///    RegisterCommand → RegisterCommandHandler
/// 
/// 4. PIPELINE BEHAVIOURS:
///    → ValidationBehaviour (validate command)
///    → PerformanceBehaviour (measure time)
///    → RegisterCommandHandler.Handle() ← ĐANG Ở ĐÂY
/// 
/// 5. HANDLER XỬ LÝ:
///    - Gọi IdentityService
///    - Tạo user trong database
///    - Trả về Result<string>
/// 
/// 6. CONTROLLER TRẢ RESPONSE:
///    if (result.IsSuccess) return Ok(result.Value);
///    else return BadRequest(result.Error);
/// 
/// DEPENDENCY INJECTION:
/// - Handler nhận dependencies qua constructor
/// - DI container tự động inject
/// - Ví dụ: IIdentityService, IRepository, IUnitOfWork...
/// 
/// VÍ DỤ:
/// public class RegisterCommandHandler
/// {
///     private readonly IIdentityService _identityService;
///     
///     public RegisterCommandHandler(IIdentityService identityService)
///     {
///         _identityService = identityService;
///     }
/// }
/// 
/// REGISTRATION:
/// services.AddMediatR(Assembly.GetExecutingAssembly());
/// → Tự động scan và register tất cả handlers
/// 
/// HANDLER RESPONSIBILITIES:
/// - Orchestrate business logic (điều phối)
/// - Call domain services
/// - Call repositories
/// - Return Result (success/failure)
/// 
/// HANDLER KHÔNG NÊN:
/// - Trực tiếp query database (dùng repository)
/// - Chứa validation logic (dùng validator)
/// - Xử lý HTTP concerns (status code, headers...)
/// - Throw exceptions (trả về Result thay vì)
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    /// <summary>
    /// IDENTITY SERVICE - Service xử lý authentication
    /// 
    /// GIẢI THÍCH:
    /// - Interface từ Infrastructure layer
    /// - Implement bởi IdentityService (ASP.NET Identity)
    /// - Xử lý: Register, Login, Password, Roles...
    /// 
    /// TẠI SAO DÙNG SERVICE?
    /// - Application layer KHÔNG biết implementation
    /// - Infrastructure implement với ASP.NET Identity
    /// - Có thể đổi implementation (Firebase, Auth0...)
    /// - Testability (mock IIdentityService)
    /// 
    /// METHODS:
    /// - RegisterAsync(): Tạo user mới
    /// - LoginAsync(): Đăng nhập
    /// - RefreshTokenAsync(): Refresh JWT token
    /// - RevokeRefreshTokenAsync(): Revoke token
    /// 
    /// VÍ DỤ:
    /// var result = await _identityService.RegisterAsync(
    ///     email: "user@example.com",
    ///     userName: "john_doe",
    ///     password: "SecurePass123!",
    ///     firstName: "John",
    ///     lastName: "Doe"
    /// );
    /// 
    /// if (result.IsSuccess)
    ///     userId = result.Value; // "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// else
    ///     error = result.Error;  // { Code: "CONF_8001", Message: "Email already exists" }
    /// </summary>
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor - DI inject dependencies
    /// 
    /// FLOW:
    /// 1. MediatR tạo handler instance
    /// 2. DI container resolve IIdentityService
    /// 3. Inject vào constructor
    /// 4. Handler ready to use
    /// 
    /// VÍ DỤ REGISTRATION (Startup/Program.cs):
    /// services.AddScoped<IIdentityService, IdentityService>();
    /// services.AddMediatR(Assembly.GetExecutingAssembly());
    /// 
    /// LIFETIME:
    /// - Handler: Transient (tạo mới mỗi request)
    /// - IIdentityService: Scoped (1 instance per request)
    /// </summary>
    /// <param name="identityService">Identity service từ DI</param>
    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// HANDLE METHOD - Xử lý RegisterCommand
    /// 
    /// GIẢI THÍCH:
    /// - Method chính của handler
    /// - Được gọi bởi MediatR
    /// - Nhận Command → Trả về Result
    /// 
    /// FLOW CHI TIẾT:
    /// 
    /// 1. NHẬN COMMAND (đã validated):
    ///    request = {
    ///      Email: "user@example.com",
    ///      UserName: "john_doe",
    ///      Password: "SecurePass123!",
    ///      ...
    ///    }
    /// 
    /// 2. GỌI IDENTITY SERVICE:
    ///    var result = await _identityService.RegisterAsync(...);
    /// 
    /// 3. IDENTITY SERVICE XỬ LÝ:
    ///    - Check email exists? → Error.Conflict
    ///    - Check username exists? → Error.Conflict
    ///    - Create user in database
    ///    - Assign default role ("User")
    ///    - Return Result<string> (userId)
    /// 
    /// 4. HANDLER TRẢ VỀ RESULT:
    ///    return result;
    /// 
    /// 5. CONTROLLER NHẬN RESULT:
    ///    if (result.IsSuccess)
    ///        return Ok(new { userId = result.Value });
    ///    else
    ///        return result.Error.Type switch {
    ///            ErrorType.Conflict => Conflict(result.Error),
    ///            ErrorType.Validation => BadRequest(result.Error),
    ///            _ => StatusCode(500, result.Error)
    ///        };
    /// 
    /// VÍ DỤ SUCCESS:
    /// 
    /// Input:
    /// {
    ///   "email": "newuser@example.com",
    ///   "userName": "new_user",
    ///   "password": "SecurePass123!",
    ///   "confirmPassword": "SecurePass123!",
    ///   "firstName": "New",
    ///   "lastName": "User"
    /// }
    /// 
    /// Output:
    /// Result.IsSuccess = true
    /// Result.Value = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// 
    /// HTTP Response:
    /// Status: 200 OK
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    /// }
    /// 
    /// VÍ DỤ FAILURE (Email exists):
    /// 
    /// Input:
    /// {
    ///   "email": "existing@example.com",  ← Email đã tồn tại
    ///   ...
    /// }
    /// 
    /// Output:
    /// Result.IsSuccess = false
    /// Result.Error = {
    ///   Code: "CONF_8001",
    ///   Message: "Email is already in use",
    ///   Type: ErrorType.Conflict
    /// }
    /// 
    /// HTTP Response:
    /// Status: 409 Conflict
    /// {
    ///   "error": {
    ///     "code": "CONF_8001",
    ///     "message": "Email is already in use",
    ///     "type": "Conflict"
    ///   }
    /// }
    /// 
    /// VÍ DỤ FAILURE (Validation - Password weak):
    /// 
    /// Input:
    /// {
    ///   "password": "weak",  ← Password quá yếu
    ///   ...
    /// }
    /// 
    /// Output (từ ValidationBehaviour - throw exception trước khi vào handler):
    /// ValidationException {
    ///   Errors = {
    ///     "Password": [
    ///       "Password must be at least 8 characters",
    ///       "Password must contain at least one uppercase letter",
    ///       ...
    ///     ]
    ///   }
    /// }
    /// 
    /// HTTP Response:
    /// Status: 400 Bad Request
    /// {
    ///   "error": {
    ///     "code": "Validation.Failed",
    ///     "message": "One or more validation errors occurred",
    ///     "errors": {
    ///       "Password": [
    ///         "Password must be at least 8 characters",
    ///         "Password must contain at least one uppercase letter"
    ///       ]
    ///     }
    ///   }
    /// }
    /// 
    /// ERROR HANDLING STRATEGY:
    /// 
    /// Handler KHÔNG throw exception (trừ unexpected errors)
    /// Handler TRẢ VỀ Result:
    /// - Success: Result.Success(userId)
    /// - Failure: Result.Failure(Error.Conflict(...))
    /// 
    /// Controller CONVERT Result → HTTP Response:
    /// - IsSuccess → 200 OK
    /// - ErrorType.Conflict → 409 Conflict
    /// - ErrorType.NotFound → 404 Not Found
    /// - ErrorType.Validation → 400 Bad Request
    /// - ErrorType.Unauthorized → 401 Unauthorized
    /// 
    /// LOGGING:
    /// Handler KHÔNG trực tiếp log
    /// Logging được handle bởi:
    /// - PerformanceBehaviour (slow requests)
    /// - ExceptionMiddleware (errors)
    /// - Serilog (structured logging)
    /// 
    /// TRANSACTION:
    /// RegisterAsync đã có transaction internally (trong IdentityService)
    /// Nếu cần transaction với nhiều operations:
    /// 
    /// await _unitOfWork.BeginTransactionAsync();
    /// try
    /// {
    ///     var result = await _identityService.RegisterAsync(...);
    ///     await _someOtherRepository.AddAsync(...);
    ///     await _unitOfWork.CommitTransactionAsync();
    /// }
    /// catch
    /// {
    ///     await _unitOfWork.RollbackTransactionAsync();
    ///     throw;
    /// }
    /// </summary>
    /// <param name="request">RegisterCommand object (đã validated)</param>
    /// <param name="cancellationToken">Token để cancel operation</param>
    /// <returns>Result với userId nếu success, hoặc Error nếu failure</returns>
    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // GỌI IDENTITY SERVICE để tạo user
        // Service sẽ:
        // 1. Check email/username đã tồn tại chưa
        // 2. Hash password
        // 3. Insert user vào database
        // 4. Assign role mặc định
        // 5. Return userId hoặc Error
        var result = await _identityService.RegisterAsync(
            email: request.Email,
            userName: request.UserName,
            password: request.Password,
            firstName: request.FirstName,
            lastName: request.LastName
        );

        // TRẢ VỀ RESULT
        // Result có thể là:
        // - Success: Result<string> với Value = userId
        // - Failure: Result<string> với Error = error details
        return result;
    }
}