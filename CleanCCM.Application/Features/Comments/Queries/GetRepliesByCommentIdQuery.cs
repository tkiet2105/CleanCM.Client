using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Comments.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Comments.Queries;

public record GetRepliesByCommentIdQuery(Guid ParentCommentId)
    : IRequest<Result<List<CommentDto>>>;
public class GetRepliesByCommentIdQueryHandler
    : IRequestHandler<GetRepliesByCommentIdQuery, Result<List<CommentDto>>>
{
    private readonly ICommentRepository _commentRepository;

    public GetRepliesByCommentIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<List<CommentDto>>> Handle(
        GetRepliesByCommentIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ParentCommentId == Guid.Empty)
        {
            return Result<List<CommentDto>>.Failure(
                Error.Validation(BaseErrors.NotFoundById, "ParentCommentId is required"));
        }

        var comments = await _commentRepository.GetRepliesAsync(
            request.ParentCommentId, cancellationToken);

        var list = comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                UserId = c.UserId,
                Content = c.Content,
                ParentCommentId = c.ParentCommentId,
                CreatedAt = c.CreatedAt,
                Replies = new List<CommentDto>()
            })
            .ToList();

        return Result<List<CommentDto>>.Success(list);
    }
}