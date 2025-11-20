using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Images.Commands;

public record DeleteImageCommand(Guid Id) : IRequest<Result>;

public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Result>
{
    private readonly IRepository<Image> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteImageCommandHandler(
        IRepository<Image> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (image == null)
            return Result.Failure(Error.NotFound(BaseErrors.NotFoundById, "Image not found"));

        _repository.Remove(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}