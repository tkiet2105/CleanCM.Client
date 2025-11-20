using AutoMapper;
using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Images.Commands;
using CleanCCM.Application.Features.Images.Queries;
using CleanCCM.Application.Features.Tags.Queries;
using CleanCCM.Shared.Images.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

public class ImagesController : BaseApiController
{
    private readonly IMapper _mapper;

    public ImagesController(IMapper mapper)
    {
        _mapper = mapper;
    }

    // GET: api/images/get-by-id?id=...
    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new GetImageByIdQuery(id));
        return HandleResult(result);
    }

    // GET: api/images/by-product?productId=...
    [HttpGet("by-product")]
    public async Task<IActionResult> GetByProduct([FromQuery] Guid productId)
    {
        var result = await Mediator.Send(new GetImagesByProductIdQuery(productId));
        return HandleResult(result);
    }

    // GET: api/images/primary?productId=...
    [HttpGet("primary")]
    public async Task<IActionResult> GetPrimary([FromQuery] Guid productId)
    {
        var result = await Mediator.Send(new GetPrimaryImageQuery(productId));
        return HandleResult(result);
    }

    // POST: api/images/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateImageRequest request)
    {
        var command = _mapper.Map<CreateImageCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    // POST: api/images/update
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateImageRequest request)
    {
        var command = _mapper.Map<UpdateImageCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteImageCommand(id));
        return HandleResult(result);
    }
}
