using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Reactions.Commands;

public record CreateReactionCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public ReactionType Type { get; init; }
}

public class CreateReactionCommandHandler : IRequestHandler<CreateReactionCommand, Result<Guid>>
{
    private readonly IRepository<Reaction> _reactionRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateReactionCommandHandler(
        IRepository<Reaction> reactionRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _reactionRepository = reactionRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result<Guid>.Failure(Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        var productExists = await _productRepository.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
            return Result<Guid>.Failure(Error.NotFound(BaseErrors.NotFoundById, "Product not found"));

        var existingReaction = await _reactionRepository.FirstOrDefaultAsync(
            r => r.ProductId == request.ProductId && r.UserId == _currentUserService.UserId,
            cancellationToken);

        if (existingReaction != null)
        {
            existingReaction.ChangeType(request.Type);
            _reactionRepository.Update(existingReaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(existingReaction.Id);
        }

        var reaction = Reaction.Create(request.ProductId, _currentUserService.UserId, request.Type);
        await _reactionRepository.AddAsync(reaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reaction.Id);
    }
}