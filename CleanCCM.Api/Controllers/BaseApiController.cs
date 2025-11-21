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
            var error = new ApiError
            {
                Code = "System.NullResult",
                Category = "Server",
                Detail = "Result was null"
            };

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResult.Fail(error, "Internal server error"));
        }

        if (result.IsSuccess)
        {
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
                Code = "System.NullResult",
                Category = "Server",
                Detail = "Result was null"
            };

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResult<T>.Fail(apiError, "Internal server error")
            );
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
    protected IActionResult MapErrorToHttpResponse<T>(Error error)
    {
        var apiError = new ApiError
        {
            Code = error.Code,              // VD: "Base.NotFoundById"
            Category = error.Type.ToString(), // VD: "NotFound", "Validation"
            Detail = error.Message       // optional: mô tả thêm
        };

        return error.Type switch
        {
            ErrorType.Validation => BadRequest(ApiResult.Fail(apiError, "Dữ liệu không hợp lệ")),
            ErrorType.NotFound => NotFound(ApiResult.Fail(apiError, "Không tìm thấy dữ liệu")),
            ErrorType.Conflict => Conflict(ApiResult.Fail(apiError, "Xung đột dữ liệu")),
            ErrorType.Business => UnprocessableEntity(ApiResult.Fail(apiError, "Không thể thực hiện yêu cầu")),
            ErrorType.Unauthorized => Unauthorized(ApiResult.Fail(apiError, "Chưa đăng nhập")),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden,
                ApiResult.Fail(apiError, "Không có quyền thực hiện")),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                ApiResult.Fail(apiError, "Lỗi hệ thống"))
        };
    }
}