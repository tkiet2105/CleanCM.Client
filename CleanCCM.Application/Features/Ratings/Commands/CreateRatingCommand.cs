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
    private readonly IRepository<Rating> _ratingRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateRatingCommandHandler(
        IRepository<Rating> ratingRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _ratingRepository = ratingRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateRatingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result<Guid>.Failure(Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (request.Score < 1 || request.Score > 5)
            return Result<Guid>.Failure(Error.Validation(BaseErrors.OutOfRange, "Score must be between 1 and 5"));

        var productExists = await _productRepository.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
            return Result<Guid>.Failure(Error.NotFound(BaseErrors.NotFoundById, "Product not found"));

        var existingRating = await _ratingRepository.FirstOrDefaultAsync(
            r => r.ProductId == request.ProductId && r.UserId == _currentUserService.UserId,
            cancellationToken);

        if (existingRating != null)
        {
            existingRating.Update(request.Score, request.Review);
            _ratingRepository.Update(existingRating);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(existingRating.Id);
        }

        var rating = Rating.Create(request.ProductId, _currentUserService.UserId, request.Score, request.Review);
        await _ratingRepository.AddAsync(rating, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(rating.Id);
    }
}