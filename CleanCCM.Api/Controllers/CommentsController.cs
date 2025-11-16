using CleanCCM.API.Controllers;
using CleanCCM.Application.Features.Comments.Commands;
using CleanCCM.Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCCM.Api.Controllers;

/// API/Controllers/CommentsController.cs
public class CommentsController : BaseApiController
{




    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "ID mismatch" });

        var result = await Mediator.Send(command);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteCommentCommand(id));
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}