using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Tags.Commands;

public record CreateTagCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}

public class CreateTagCommandHandler
    : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly IRepository<Tag> _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateTagCommandHandler(
        IRepository<Tag> tagRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsAdmin)
        {
            return Result<Guid>.Failure(
                Error.Forbidden(BaseErrors.Required, "Only administrator can create tag"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Guid>.Failure(
                Error.Validation(BaseErrors.InvalidValue, "Tag name is required"));
        }

        var tag = Tag.Create(request.Name, request.Color);

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }
}
