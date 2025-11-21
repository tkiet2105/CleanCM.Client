using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Categories.Commands;

public record UpdateCategoryCommand : IRequest<Result>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public int DisplayOrder { get; init; } = 0;
    public bool IsActive { get; init; } = true;
}

public class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsInRole("Administrator"))
        {
            return Result.Failure(
                Error.Forbidden(BaseErrors.Required, "Only administrator can update category"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(
                Error.Validation(BaseErrors.InvalidValue, "Name is required"));
        }

        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Category not found"));
        }

        category.UpdateInfo(request.Name, request.Description);
        category.SetIcon(request.Icon);
        category.SetDisplayOrder(request.DisplayOrder);

        if (request.IsActive)
            category.Activate();
        else
            category.Deactivate();

        // Check slug trùng (exclude chính nó)
        if (!string.IsNullOrWhiteSpace(category.Slug))
        {
            var exists = await _categoryRepository.IsSlugExistsAsync(
                category.Slug!,
                excludeId: category.Id,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result.Failure(
                    Error.Conflict(BaseErrors.AlreadyExists, "Category slug already exists"));
            }
        }

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}