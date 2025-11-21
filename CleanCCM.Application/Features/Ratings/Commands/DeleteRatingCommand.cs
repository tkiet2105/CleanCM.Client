using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Features.Ratings.Commands;

public record DeleteRatingCommand(Guid Id) : IRequest<Result>;

public class DeleteRatingCommandHandler : IRequestHandler<DeleteRatingCommand, Result>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRatingCommandHandler(
        IRatingRepository ratingRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _ratingRepository = ratingRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteRatingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result.Failure(
                Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        var rating = await _ratingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (rating == null)
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Rating not found"));

        if (rating.UserId != _currentUserService.UserId)
            return Result.Failure(
                Error.Forbidden(BaseErrors.Required, "You can only delete your own rating"));

        _ratingRepository.Remove(rating);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
