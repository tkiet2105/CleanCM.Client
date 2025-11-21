using AutoMapper;
using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Products.Commands;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Application.Features.Tags.Commands;
using CleanCCM.Application.Features.Tags.Queries;
using CleanCCM.Shared.Tags.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/TagsController.cs

public class TagsController : BaseApiController
{
    // Ai cũng xem được
    [HttpGet("get-all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllTagsQuery());
        return HandleResult(result);
    }

    // Ai cũng xem được
    [HttpGet("get-by-id")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new GetTagByIdQuery(id));
        return HandleResult(result);
    }

    // Chỉ Administrator được tạo
    [HttpPost("create")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        var command = Mapper.Map<CreateTagCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    // Chỉ Administrator được sửa
    [HttpPost("update")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update([FromBody] UpdateTagRequest request)
    {
        var command = Mapper.Map<UpdateTagCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    // Chỉ Administrator được xoá
    [HttpPost("delete")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteTagCommand(id));
        return HandleResult(result);
    }
}

