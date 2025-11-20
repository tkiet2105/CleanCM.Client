using AutoMapper;
using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Products.Commands;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Shared.Products.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/ProductsController.cs
public class ProductsController : BaseApiController
{
    private readonly IMapper _mapper;

    public ProductsController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProductRequest request)
    {
        var query = _mapper.Map<GetAllProductQuery>(request);

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var command = _mapper.Map<CreateProductCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProductRequest request)
    {
        var command = _mapper.Map<UpdateProductCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteProductCommand(id));
        return HandleResult(result);
    }
}


