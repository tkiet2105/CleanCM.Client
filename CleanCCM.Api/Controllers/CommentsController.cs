using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Comments.Commands;
using CleanCCM.Application.Features.Comments.Queries;
using CleanCCM.Shared.Comments.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/CommentsController.cs
public class CommentsController : BaseApiController
{

    /// <summary>
    /// Lấy danh sách comment theo Product (phân trang root comments).
    /// Mỗi comment trong page sẽ có đầy đủ Replies.
    /// </summary>
    /// GET: api/comments/by-product?ProductId=...&PageNumber=1&PageSize=10
    [HttpGet("by-product")]
    public async Task<IActionResult> GetByProduct([FromQuery] GetCommentsByProductRequest request)
    {
        var query = Mapper.Map<GetCommentsByProductIdQuery>(request);

        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy toàn bộ comment của 1 user (không phân trang, nếu cần thì sau này làm thêm).
    /// </summary>
    /// GET: api/comments/by-user?userId=...
    [HttpGet("by-user")]
    public async Task<IActionResult> GetByUser([FromQuery] string userId)
    {
        var result = await Mediator.Send(new GetCommentsByUserIdQuery(userId));
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy replies trực tiếp của một comment (1 cấp).
    /// </summary>
    /// GET: api/comments/replies?parentCommentId=...
    [HttpGet("replies")]
    public async Task<IActionResult> GetReplies([FromQuery] Guid parentCommentId)
    {
        var result = await Mediator.Send(new GetRepliesByCommentIdQuery(parentCommentId));
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy tổng số root comment của 1 product.
    /// </summary>
    /// GET: api/comments/count?productId=...
    [HttpGet("count")]
    public async Task<IActionResult> GetCount([FromQuery] Guid productId)
    {
        var result = await Mediator.Send(new GetCommentCountByProductIdQuery(productId));
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo comment mới.
    /// </summary>
    /// POST: api/comments/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest request)
    {
        var command = Mapper.Map<CreateCommentCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật nội dung comment.
    /// </summary>
    /// POST: api/comments/update
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateCommentRequest request)
    {
        var command = Mapper.Map<UpdateCommentCommand>(request);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Xoá comment.
    /// </summary>
    /// POST: api/comments/delete?id=...
    [HttpPost("delete")]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var result = await Mediator.Send(new DeleteCommentCommand(id));
        return HandleResult(result);
    }
}