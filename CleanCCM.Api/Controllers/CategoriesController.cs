using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Categories.Commands;
using CleanCCM.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/CategoriesController.cs
public class CategoriesController : BaseApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery]GetAllCategoriesQuery queries)
    {
        var result = await Mediator.Send(queries);
        return HandleResult(result);

    }

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new GetCategoryByIdQuery(id));
        return HandleResult(result);

    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);

    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);

    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteCategoryCommand(id));
        return HandleResult(result);

    }
}