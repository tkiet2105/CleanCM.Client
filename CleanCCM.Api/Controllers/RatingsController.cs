using AutoMapper;
using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Ratings.Commands;
using CleanCCM.Application.Features.Ratings.Queries;
using CleanCCM.Shared.Rating.Requests;
using CleanCCM.Shared.Rating.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

public class RatingsController : BaseApiController
{
  

    /// <summary>
    /// Lấy danh sách rating theo product (có phân trang).
    /// </summary>
    /// GET: api/ratings/by-product?ProductId=...&PageNumber=1&PageSize=10
    [HttpGet("by-product")]
    public async Task<IActionResult> GetByProduct([FromQuery] GetRatingsByProductRequest request)
    {
        var query = Mapper.Map<GetRatingsByProductIdQuery>(request);

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy rating của user hiện tại cho 1 product (nếu có).
    /// </summary>
    /// GET: api/ratings/my-rating?productId=...
    [HttpGet("my-rating")]
    public async Task<IActionResult> GetMyRating([FromQuery] Guid productId)
    {
        var result = await Mediator.Send(new GetUserRatingForProductQuery(productId));
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy summary rating (average, distribution...) cho product.
    /// </summary>
    /// GET: api/ratings/summary?productId=...
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] Guid productId)
    {
        var result = await Mediator.Send(new GetRatingSummaryByProductIdQuery(productId));
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo rating mới cho product (user hiện tại).
    /// </summary>
    /// POST: api/ratings/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRatingRequest request)
    {
        var command = Mapper.Map<CreateRatingCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật rating (chỉ cho phép owner).
    /// </summary>
    /// POST: api/ratings/update
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateRatingRequest request)
    {
        var command = Mapper.Map<UpdateRatingCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá rating (chỉ cho phép owner).
    /// </summary>
    /// POST: api/ratings/delete?id=...
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteRatingCommand(id));
        return HandleResult(result);
    }
}
