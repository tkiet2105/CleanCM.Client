using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Features.Tags.Commands;

public record CreateTagCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly IRepository<Tag> _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTagCommandHandler(
        IRepository<Tag> tagRepository,
        IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = Tag.Create(request.Name, request.Color);

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }
}