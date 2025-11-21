using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;

namespace CleanCCM.Application.Features.Categories.Commands;

public record CreateCategoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public int DisplayOrder { get; init; } = 0;
}

public class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsInRole("Administrator"))
        {
            return Result<Guid>.Failure(
                Error.Forbidden(BaseErrors.Required, "Only administrator can create category"));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Guid>.Failure(
                Error.Validation(BaseErrors.InvalidValue, "Name is required"));
        }

        var category = Category.Create(
            name: request.Name,
            icon: request.Icon,
            description: request.Description,
            displayOrder: request.DisplayOrder
        );

        // Kiểm tra slug trùng
        if (!string.IsNullOrWhiteSpace(category.Slug))
        {
            var exists = await _categoryRepository.IsSlugExistsAsync(
                category.Slug!,
                excludeId: null,
                cancellationToken: cancellationToken);

            if (exists)
            {
                return Result<Guid>.Failure(
                    Error.Conflict(BaseErrors.AlreadyExists, "Category slug already exists"));
            }
        }

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }
}