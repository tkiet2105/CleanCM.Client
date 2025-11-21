using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Features.Ratings.Commands;

public record UpdateRatingCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public int Score { get; init; }
    public string? Review { get; init; }
}

public class UpdateRatingCommandHandler : IRequestHandler<UpdateRatingCommand, Result>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRatingCommandHandler(
        IRatingRepository ratingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _ratingRepository = ratingRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateRatingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result.Failure(
                Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (request.Score < 1 || request.Score > 5)
            return Result.Failure(
                Error.Validation(BaseErrors.OutOfRange, "Score must be between 1 and 5"));

        var rating = await _ratingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (rating == null)
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Rating not found"));

        if (rating.UserId != _currentUserService.UserId)
            return Result.Failure(
                Error.Forbidden(BaseErrors.Required, "You can only update your own rating"));

        rating.Update(request.Score, request.Review);

        _ratingRepository.Update(rating);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
