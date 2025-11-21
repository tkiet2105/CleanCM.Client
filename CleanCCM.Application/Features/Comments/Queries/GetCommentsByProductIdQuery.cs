using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Comments.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Comments.Queries;

public record GetCommentsByProductIdQuery : IRequest<Result<PaginatedList<CommentDto>>>
{
    public Guid ProductId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetCommentsByProductIdQueryHandler
    : IRequestHandler<GetCommentsByProductIdQuery, Result<PaginatedList<CommentDto>>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsByProductIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<PaginatedList<CommentDto>>> Handle(
        GetCommentsByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<PaginatedList<CommentDto>>.Failure(
                Error.Validation(BaseErrors.InvalidValue, "ProductId is required"));
        }

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        // Lấy tất cả comments phẳng của product
        var comments = await _commentRepository.GetByProductIdAsync(
            request.ProductId, cancellationToken);

        var flatDtos = comments
            .OrderBy(c => c.CreatedAt)
            .Select(Map)
            .ToList();

        var lookup = flatDtos.ToDictionary(c => c.Id);

        // Gắn Replies
        foreach (var dto in flatDtos)
        {
            if (dto.ParentCommentId is Guid parentId &&
                lookup.TryGetValue(parentId, out var parentDto))
            {
                parentDto.Replies.Add(dto);
            }
        }

        // Chỉ phân trang trên root comment
        var rootComments = flatDtos
            .Where(c => c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        var totalCount = rootComments.Count;

        var itemsPage = rootComments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var paged = new PaginatedList<CommentDto>(
            itemsPage,
            totalCount,
            pageNumber,
            pageSize
        );

        return Result<PaginatedList<CommentDto>>.Success(paged);
    }

    private static CommentDto Map(Comment c)
        => new()
        {
            Id = c.Id,
            ProductId = c.ProductId,
            UserId = c.UserId,
            Content = c.Content,
            ParentCommentId = c.ParentCommentId,
            CreatedAt = c.CreatedAt,
            Replies = new List<CommentDto>()
        };
}