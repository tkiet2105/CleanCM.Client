using CleanCCM.Application.Common.Models;
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

    protected IActionResult HandleResult(Result result)
    {
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

 
}