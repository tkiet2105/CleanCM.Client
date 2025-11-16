using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Comments.Commands;

public record CreateCommentCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
}

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly IRepository<Comment> _commentRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommentCommandHandler(
        IRepository<Comment> commentRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
            return Result<Guid>.Failure(Error.Unauthorized(BaseErrors.Required, "User must be authenticated"));

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result<Guid>.Failure(Error.Validation(BaseErrors.Required, "Content is required"));

        var productExists = await _productRepository.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
            return Result<Guid>.Failure(Error.NotFound(BaseErrors.NotFoundById, "Product not found"));

        if (request.ParentCommentId.HasValue)
        {
            var parentExists = await _commentRepository.AnyAsync(c => c.Id == request.ParentCommentId.Value, cancellationToken);
            if (!parentExists)
                return Result<Guid>.Failure(Error.NotFound(BaseErrors.NotFoundById, "Parent comment not found"));
        }

        var comment = Comment.Create(
            request.ProductId,
            _currentUserService.UserId,
            request.Content,
            request.ParentCommentId);

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(comment.Id);
    }
}