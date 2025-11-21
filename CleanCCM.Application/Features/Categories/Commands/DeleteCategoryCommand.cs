using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;


public class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsInRole("Administrator"))
        {
            return Result.Failure(
                Error.Forbidden(BaseErrors.Required, "Only administrator can delete category"));
        }

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Category not found"));
        }

        _categoryRepository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}