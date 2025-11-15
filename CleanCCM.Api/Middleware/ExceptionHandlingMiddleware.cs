using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CleanCCM.Application.Common.Exceptions;

namespace CleanCCM.Infrastructure.Middleware;

/// <summary>
/// MIDDLEWARE XỬ LÝ EXCEPTIONS TOÀN CỤC
/// 
/// GIẢI THÍCH CHO JUNIOR:
/// 
/// EXCEPTION HANDLING MIDDLEWARE LÀ GÌ?
/// - Middleware bắt TẤT CẢ exceptions trong pipeline
/// - Convert exceptions thành HTTP responses
/// - Log errors
/// - Trả về error format thống nhất
/// 
/// FLOW:
/// 
/// Request
///   ↓
/// ExceptionHandlingMiddleware (try)
///   ↓
/// Other Middlewares
///   ↓
/// Controller (throw exception)
///   ↓
/// ExceptionHandlingMiddleware (catch)
///   ↓
/// Return Error Response
/// 
/// VÍ DỤ:
/// 
/// // Controller
/// var user = await _repository.GetByIdAsync(id);
/// if (user == null)
///     throw new NotFoundException("User", id);
/// 
/// // Middleware catch
/// → 404 Not Found Response
/// {
///   "statusCode": 404,
///   "error": {
///     "code": "NotFound",
///     "message": "Entity \"User\" (xxx) was not found."
///   }
/// }
/// 
/// TẠI SAO CẦN?
/// - Tránh duplicate error handling trong mọi controller
/// - Format error response thống nhất
/// - Centralized logging
/// - Hide sensitive error details từ client
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// INVOKE METHOD - Bắt và xử lý exceptions
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // GỌI MIDDLEWARE TIẾP THEO
            await _next(context);
        }
        catch (Exception exception)
        {
            // BẮT EXCEPTION VÀ XỬ LÝ
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// XỬ LÝ EXCEPTION VÀ TRẢ VỀ RESPONSE
    /// 
    /// GIẢI THÍCH:
    /// - Xác định exception type
    /// - Map sang HTTP status code
    /// - Tạo error response
    /// - Write JSON response
    /// 
    /// EXCEPTION TYPES:
    /// 
    /// 1. ValidationException (FluentValidation)
    ///    → 400 Bad Request
    ///    → Chứa validation errors theo field
    /// 
    /// 2. NotFoundException
    ///    → 404 Not Found
    ///    → Entity không tồn tại
    /// 
    /// 3. UnauthorizedAccessException
    ///    → 401 Unauthorized
    ///    → Không có quyền truy cập
    /// 
    /// 4. Exception (Generic)
    ///    → 500 Internal Server Error
    ///    → Lỗi không mong đợi
    /// 
    /// VÍ DỤ RESPONSE:
    /// 
    /// ValidationException:
    /// {
    ///   "statusCode": 400,
    ///   "error": {
    ///     "code": "Validation.Failed",
    ///     "message": "One or more validation errors occurred",
    ///     "errors": {
    ///       "Email": ["Invalid format"],
    ///       "Password": ["Too short"]
    ///     }
    ///   }
    /// }
    /// 
    /// NotFoundException:
    /// {
    ///   "statusCode": 404,
    ///   "error": {
    ///     "code": "NotFound",
    ///     "message": "Entity \"User\" (xxx) was not found."
    ///   }
    /// }
    /// 
    /// Generic Exception:
    /// {
    ///   "statusCode": 500,
    ///   "error": {
    ///     "code": "InternalServerError",
    ///     "message": "An error occurred while processing your request"
    ///   }
    /// }
    /// </summary>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // SET CONTENT TYPE
        context.Response.ContentType = "application/json";

        // XÁC ĐỊNH STATUS CODE VÀ ERROR RESPONSE
        var (statusCode, errorResponse) = exception switch
        {
            // VALIDATION EXCEPTION (FluentValidation)
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Error = new ErrorDetail
                    {
                        Code = "Validation.Failed",
                        Message = "One or more validation errors occurred",
                        Errors = validationException.Errors
                    }
                }
            ),

            // NOT FOUND EXCEPTION
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Error = new ErrorDetail
                    {
                        Code = "NotFound",
                        Message = notFoundException.Message
                    }
                }
            ),

            // UNAUTHORIZED ACCESS EXCEPTION
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Error = new ErrorDetail
                    {
                        Code = "Unauthorized",
                        Message = "You are not authorized to perform this action"
                    }
                }
            ),

            // GENERIC EXCEPTION (Catch-all)
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = new ErrorDetail
                    {
                        Code = "InternalServerError",
                        Message = "An error occurred while processing your request"
                        // KHÔNG trả về exception.Message trong production (security)
                    }
                }
            )
        };

        // SET STATUS CODE
        context.Response.StatusCode = statusCode;

        // SERIALIZE VÀ WRITE RESPONSE
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// ERROR RESPONSE DTO
    /// </summary>
    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public ErrorDetail Error { get; set; } = null!;
    }

    /// <summary>
    /// ERROR DETAIL DTO
    /// </summary>
    private class ErrorDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public IDictionary<string, string[]>? Errors { get; set; }  // Cho ValidationException
    }
}