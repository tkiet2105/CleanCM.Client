using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Comments.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Comments.Queries;


public record GetCommentCountByProductIdQuery(Guid ProductId)
    : IRequest<Result<int>>;
public class GetCommentCountByProductIdQueryHandler
    : IRequestHandler<GetCommentCountByProductIdQuery, Result<int>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentCountByProductIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<int>> Handle(
        GetCommentCountByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<int>.Failure(
                Error.Validation(BaseErrors.InvalidDate, "ProductId is required"));
        }

        var count = await _commentRepository.GetCommentCountAsync(
            request.ProductId, cancellationToken);

        return Result<int>.Success(count);
    }
}