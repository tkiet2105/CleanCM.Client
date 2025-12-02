using AutoMapper;
using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Products.Commands;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Shared.Products.Requests;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/ProductsController.cs
public class ProductsController : BaseApiController
{
    
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProductRequest request)
    {
        var query = Mapper.Map<GetAllProductQuery>(request);

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
    [HttpPost("summary")]
    public async Task<IActionResult> GetSummaryProduct([FromBody] GetSummaryProductRequest request)
    {
        var query = new GetSummaryProductQuery(
              request.CategoryKeys,
              request.TagKeys,
              request.WardKeys
             );
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }
    /// <summary>
    /// Lọc sản phẩm theo CategoryKey, TagKey, WardKey (có thể kết hợp)
    /// </summary>
    [HttpPost("filter-by-keys")]  
    public async Task<IActionResult> FilterByKeys([FromBody] GetProductsByKeyRequest request)  // ← Đổi từ FromQuery
    {
        var query = new GetProductsByKeyQuery(
            request.CategoryKeys,
            request.TagKeys,
            request.WardKeys,
            request.PageNumber,
            request.PageSize);

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
        var command = Mapper.Map<CreateProductCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProductRequest request)
    {
        var command = Mapper.Map<UpdateProductCommand>(request);

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


