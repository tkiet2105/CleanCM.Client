using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Comments.Commands;

public record CreateCommentCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
}

public class CreateCommentCommandHandler
    : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentCommandHandler(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result<Guid>.Failure(
                Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (request.ProductId == Guid.Empty)
            return Result<Guid>.Failure(
                Error.Validation(BaseErrors.InvalidId, "ProductId is required"));

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result<Guid>.Failure(
                Error.Validation(BaseErrors.InvalidValue, "Content is required"));

        var comment = Comment.Create(
            productId: request.ProductId,
            userId: _currentUserService.UserId!,
            content: request.Content,
            parentCommentId: request.ParentCommentId
        );

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(comment.Id);
    }
}