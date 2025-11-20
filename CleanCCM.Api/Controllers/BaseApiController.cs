using AutoMapper;
using CleanCCM.Application.Common.Models;
using CleanCCM.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.API.Controllers;

// Base controller với Result pattern, chỉ sử dụng GET và POST
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    private IMapper? _mapper;
    protected IMapper Mapper
        => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();
    //protected IActionResult HandleResult(Result result)
    //{
    //    return result.IsSuccess ? Ok(result) : BadRequest(result);
    //}

    //protected IActionResult HandleResult<T>(Result<T> result)
    //{
    //    return result.IsSuccess ? Ok(result) : BadRequest(result);
    //}


    protected IActionResult HandleResult(Result result)
    {
        if (result is null)
        {
            // Trường hợp không mong muốn → 500
            var apiError = new ApiError
            {
                Code = "SYS_0001",
                Message = "Result is null",
                Type = ErrorType.Failure.ToString()
            };

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResult.Fail(apiError));
        }

        if (result.IsSuccess)
        {
            // Không có data, chỉ cần báo OK
            return Ok(ApiResult.Ok());
        }

        return MapErrorToHttpResponse<object>(result.Error);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result is null)
        {
            var apiError = new ApiError
            {
                Code = "SYS_0001",
                Message = "Result is null",
                Type = ErrorType.Failure.ToString()
            };

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResult<T>.Fail(apiError));
        }

        if (result.IsSuccess)
        {
            return Ok(ApiResult<T>.Ok(result.Value));
        }

        return MapErrorToHttpResponse<T>(result.Error);
    }

    /// <summary>
    /// Map Error (Application) -> ApiError (Contract) + HTTP Status
    /// </summary>
    private IActionResult MapErrorToHttpResponse<T>(Error error)
    {
        // Trong Result.Success() thì Error = Error.None
        // Không nên trả Error.None ra ngoài, coi như lỗi server.
        if (error == Error.None)
        {
            var apiErrorNone = new ApiError
            {
                Code = "SYS_0002",
                Message = "Unknown error",
                Type = ErrorType.Failure.ToString()
            };

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResult<T>.Fail(apiErrorNone));
        }

        var apiError = new ApiError
        {
            Code = error.Code,
            Message = error.Message,
            Type = error.Type.ToString(),
            Metadata = error.Metadata
        };

        return error.Type switch
        {
            ErrorType.Validation =>
                BadRequest(ApiResult<T>.Fail(apiError)),                      // 400

            ErrorType.NotFound =>
                NotFound(ApiResult<T>.Fail(apiError)),                        // 404

            ErrorType.Conflict =>
                Conflict(ApiResult<T>.Fail(apiError)),                        // 409

            ErrorType.Unauthorized =>
                Unauthorized(ApiResult<T>.Fail(apiError)),                    // 401

            ErrorType.Forbidden =>
                StatusCode(StatusCodes.Status403Forbidden,                    // 403
                    ApiResult<T>.Fail(apiError)),

            ErrorType.Business =>
                StatusCode(StatusCodes.Status422UnprocessableEntity,          // 422
                    ApiResult<T>.Fail(apiError)),

            _ =>
                StatusCode(StatusCodes.Status500InternalServerError,          // 500
                    ApiResult<T>.Fail(apiError))
        };
    }
}