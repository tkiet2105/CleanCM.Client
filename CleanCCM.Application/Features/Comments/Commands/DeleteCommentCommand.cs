using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Comments.Commands;

public record DeleteCommentCommand(Guid Id) : IRequest<Result>;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result>
{
    private readonly IRepository<Comment> _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCommentCommandHandler(
        IRepository<Comment> commentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result.Failure(Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment == null)
            return Result.Failure(Error.NotFound(BaseErrors.NotFoundById, "Comment not found"));

        if (comment.UserId != _currentUserService.UserId)
            return Result.Failure(Error.Forbidden(BaseErrors.Required, "You can only delete your own comment"));

        _commentRepository.Remove(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}