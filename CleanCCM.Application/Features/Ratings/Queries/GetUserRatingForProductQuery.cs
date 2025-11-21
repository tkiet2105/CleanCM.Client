using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Ratings.DTOs;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Ratings.Queries;

public record GetUserRatingForProductQuery(Guid ProductId)
    : IRequest<Result<RatingDto?>>;


public class GetUserRatingForProductQueryHandler
    : IRequestHandler<GetUserRatingForProductQuery, Result<RatingDto?>>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserRatingForProductQueryHandler(
        IRatingRepository ratingRepository,
        ICurrentUserService currentUserService)
    {
        _ratingRepository = ratingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RatingDto?>> Handle(
        GetUserRatingForProductQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result<RatingDto?>.Failure(
                Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (request.ProductId == Guid.Empty)
            return Result<RatingDto?>.Failure(
                Error.Validation(BaseErrors.InvalidId, "ProductId is required"));

        var rating = await _ratingRepository.GetByProductAndUserAsync(
            request.ProductId,
            _currentUserService.UserId!,
            cancellationToken);

        if (rating == null)
            return Result<RatingDto?>.Success(null);

        var dto = new RatingDto
        {
            Id = rating.Id,
            ProductId = rating.ProductId,
            UserId = rating.UserId,
            Score = rating.Score,
            Review = rating.Review,
            CreatedAt = rating.CreatedAt
        };

        return Result<RatingDto?>.Success(dto);
    }
}
