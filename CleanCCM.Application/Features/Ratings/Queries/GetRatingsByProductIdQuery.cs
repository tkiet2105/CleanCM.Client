using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Ratings.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Features.Ratings.Queries;

public record GetRatingsByProductIdQuery : IRequest<Result<PaginatedList<RatingDto>>>
{
    public Guid ProductId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetRatingsByProductIdQueryHandler
    : IRequestHandler<GetRatingsByProductIdQuery, Result<PaginatedList<RatingDto>>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetRatingsByProductIdQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<Result<PaginatedList<RatingDto>>> Handle(
        GetRatingsByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<PaginatedList<RatingDto>>.Failure(
                Error.Validation(BaseErrors.InvalidId, "ProductId is required"));
        }

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var all = await _ratingRepository.GetByProductIdAsync(
            request.ProductId, cancellationToken);

        var ordered = all
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        var totalCount = ordered.Count;

        var pageItems = ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(Map)
            .ToList();

        var paged = new PaginatedList<RatingDto>(
            pageItems,
            totalCount,
            pageNumber,
            pageSize
        );

        return Result<PaginatedList<RatingDto>>.Success(paged);
    }

    private static RatingDto Map(Rating r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        UserId = r.UserId,
        Score = r.Score,
        Review = r.Review,
        CreatedAt = r.CreatedAt
    };
}
