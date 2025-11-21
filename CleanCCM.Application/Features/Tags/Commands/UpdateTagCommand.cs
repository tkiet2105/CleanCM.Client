using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Tags.Commands;

public record UpdateTagCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}

public class UpdateTagCommandHandler
    : IRequestHandler<UpdateTagCommand, Result>
{
    private readonly IRepository<Tag> _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTagCommandHandler(
        IRepository<Tag> tagRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsAdmin)
        {
            return Result.Failure(
                Error.Forbidden(BaseErrors.Required, "Only administrator can update tag"));
        }

        var tag = await _tagRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tag == null)
        {
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Tag not found"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(
                Error.Validation(BaseErrors.InvalidValue, "Tag name is required"));
        }

        tag.UpdateInfo(request.Name, request.Color);

        _tagRepository.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
