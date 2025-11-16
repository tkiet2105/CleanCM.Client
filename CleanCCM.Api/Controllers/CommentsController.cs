using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Comments.Commands;
using CleanCCM.Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/CommentsController.cs
public class CommentsController : BaseApiController
{




    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateCommentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteCommentCommand(id));
        return HandleResult(result);
    }
}