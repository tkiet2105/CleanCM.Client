using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Comments.DTOs;

namespace CleanCCM.Application.Features.Comments.Queries;

public record GetCommentsQuery(Guid ProductId) : IRequest<Result<List<CommentDto>>>;

public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, Result<List<CommentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCommentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CommentDto>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _context.Comments
            .Where(c => c.ProductId == request.ProductId && c.ParentCommentId == null)
            .Include(c => c.Replies)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                UserId = c.UserId,
                Content = c.Content,
                ParentCommentId = c.ParentCommentId,
                CreatedAt = c.CreatedAt,
                Replies = c.Replies.Select(r => new CommentDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    Content = r.Content,
                    ParentCommentId = r.ParentCommentId,
                    CreatedAt = r.CreatedAt
                }).OrderBy(r => r.CreatedAt).ToList()
            })
            .ToListAsync(cancellationToken);

        return Result<List<CommentDto>>.Success(comments);
    }
}