using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Categories.Commands;
using CleanCCM.Application.Features.Categories.Queries;
using CleanCCM.Shared.Categories.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/CategoriesController.cs
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// Lấy toàn bộ category (public – ai cũng xem được)
    /// </summary>
    [HttpGet("get-all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllCategoriesQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết category theo Id (public – ai cũng xem được)
    /// </summary>
    [HttpGet("get-by-id")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new GetCategoryByIdQuery(id));
        return HandleResult(result);
    }

    /// <summary>
    /// (Optional) Lấy danh sách category đang active (public)
    /// </summary>
    [HttpGet("get-active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var result = await Mediator.Send(new GetActiveCategoriesQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo category mới – chỉ Administrator
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var command = Mapper.Map<CreateCategoryCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật category – chỉ Administrator
    /// </summary>
    [HttpPost("update")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryRequest request)
    {
        var command = Mapper.Map<UpdateCategoryCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá category – chỉ Administrator
    /// </summary>
    [HttpPost("delete")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteCategoryCommand(id));
        return HandleResult(result);
    }
}