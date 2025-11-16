using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Reactions.Commands;

public record DeleteReactionCommand(Guid ProductId) : IRequest<Result>;

public class DeleteReactionCommandHandler : IRequestHandler<DeleteReactionCommand, Result>
{
    private readonly IRepository<Reaction> _reactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteReactionCommandHandler(
        IRepository<Reaction> reactionRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteReactionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result.Failure(Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        var reaction = await _reactionRepository.FirstOrDefaultAsync(
            r => r.ProductId == request.ProductId && r.UserId == _currentUserService.UserId,
            cancellationToken);

        if (reaction == null)
            return Result.Failure(Error.NotFound(BaseErrors.NotFoundById, "Reaction not found"));

        _reactionRepository.Remove(reaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}