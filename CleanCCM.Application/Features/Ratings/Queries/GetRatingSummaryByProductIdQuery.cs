using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Ratings.DTOs;
using CleanCCM.Domain.Common.Errors;
using MediatR;

namespace CleanCCM.Application.Features.Ratings.Queries;

public record GetRatingSummaryByProductIdQuery(Guid ProductId)
    : IRequest<Result<RatingSummaryDto>>;

public class GetRatingSummaryByProductIdQueryHandler
    : IRequestHandler<GetRatingSummaryByProductIdQuery, Result<RatingSummaryDto>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetRatingSummaryByProductIdQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<Result<RatingSummaryDto>> Handle(
        GetRatingSummaryByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<RatingSummaryDto>.Failure(
                Error.Validation(BaseErrors.InvalidId, "ProductId is required"));
        }

        var avg = await _ratingRepository.GetAverageRatingAsync(
            request.ProductId, cancellationToken);

        var distribution = await _ratingRepository.GetRatingDistributionAsync(
            request.ProductId, cancellationToken);

        var total = distribution.Values.Sum();

        var dto = new RatingSummaryDto
        {
            AverageScore = avg,
            TotalRatings = total,
            Distribution = distribution
        };

        return Result<RatingSummaryDto>.Success(dto);
    }
}