using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Ratings.Commands;

public record CreateRatingCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public int Score { get; init; }
    public string? Review { get; init; }
}

public class CreateRatingCommandHandler : IRequestHandler<CreateRatingCommand, Result<Guid>>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateRatingCommandHandler(
        IRatingRepository ratingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _ratingRepository = ratingRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result<Guid>.Failure(
                Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (request.ProductId == Guid.Empty)
            return Result<Guid>.Failure(
                Error.Validation(BaseErrors.InvalidId, "ProductId is required"));

        if (request.Score < 1 || request.Score > 5)
            return Result<Guid>.Failure(
                Error.Validation(BaseErrors.OutOfRange, "Score must be between 1 and 5"));

        var userId = _currentUserService.UserId!;

        // Dùng repo chuyên biệt
        var hasRated = await _ratingRepository.HasUserRatedAsync(
            request.ProductId, userId, cancellationToken);

        if (hasRated)
        {
            return Result<Guid>.Failure(
                Error.Conflict(BaseErrors.AlreadyExists, "You have already rated this product"));
        }

        var rating = Rating.Create(
            productId: request.ProductId,
            userId: userId,
            score: request.Score,
            review: request.Review
        );

        await _ratingRepository.AddAsync(rating, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(rating.Id);
    }
}