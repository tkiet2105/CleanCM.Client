using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCCM.Application.Common.Models;

namespace CleanCCM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleFailure(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot handle success result as failure");

        var error = result.Error!;

        return error.Type switch
        {
            ErrorType.NotFound => NotFound(new { error }),
            ErrorType.Validation => BadRequest(new { error }),
            ErrorType.Conflict => Conflict(new { error }),
            ErrorType.Unauthorized => Unauthorized(new { error }),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { error }),
            ErrorType.Business => UnprocessableEntity(new { error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error })
        };
    }
}