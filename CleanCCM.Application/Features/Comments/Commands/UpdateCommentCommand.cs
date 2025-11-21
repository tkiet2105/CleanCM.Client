using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Comments.Commands;

public record UpdateCommentCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
}

public class UpdateCommentCommandHandler
    : IRequestHandler<UpdateCommentCommand, Result>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result.Failure(
                Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result.Failure(
                Error.Validation(BaseErrors.InvalidValue, "Content is required"));

        var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment == null)
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Comment not found"));

        if (comment.UserId != _currentUserService.UserId)
            return Result.Failure(
                Error.Forbidden(BaseErrors.Required, "You can only update your own comment"));

        comment.UpdateContent(request.Content);

        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}