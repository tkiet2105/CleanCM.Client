using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Products.Commands;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Application.Features.Tags.Commands;
using CleanCCM.Application.Features.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/TagsController.cs

public class TagsController : BaseApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllTagsQuery queries)
    {
        var result = await Mediator.Send(queries);
        return HandleResult(result);
    }

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new GetTagByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateTagCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateTagCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);

    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteTagCommand(id));
        return HandleResult(result);

    }
}


