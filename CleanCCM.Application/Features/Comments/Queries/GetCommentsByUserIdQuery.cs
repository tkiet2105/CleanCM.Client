using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Comments.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Comments.Queries;

public record GetCommentsByUserIdQuery(string UserId)
    : IRequest<Result<List<CommentDto>>>;
public class GetCommentsByUserIdQueryHandler
    : IRequestHandler<GetCommentsByUserIdQuery, Result<List<CommentDto>>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsByUserIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<List<CommentDto>>> Handle(
        GetCommentsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result<List<CommentDto>>.Failure(
                Error.Validation(BaseErrors.InvalidValue, "UserId is required"));
        }

        var comments = await _commentRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        var list = comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                UserId = c.UserId,
                Content = c.Content,
                ParentCommentId = c.ParentCommentId,
                CreatedAt = c.CreatedAt,
                Replies = new List<CommentDto>() // không build tree ở đây
            })
            .ToList();

        return Result<List<CommentDto>>.Success(list);
    }
}