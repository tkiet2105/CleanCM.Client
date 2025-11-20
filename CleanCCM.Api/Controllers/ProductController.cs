using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Products.Commands;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Shared.Products.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/ProductsController.cs
public class ProductsController : BaseApiController
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProductQueryRequest queryRequest)
    {
       
        var query = Mapper.Map<GetAllProductQuery>(queryRequest);

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
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);

    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProductCommand command)
    {
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


