using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Categories.DTOs;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Categories.Queries;

public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;

public class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category == null)
        {
            return Result<CategoryDto>.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Category not found"));
        }

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            Icon = category.Icon,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            ProductCount = category.ProductCategories?.Count ?? 0
        };

        return Result<CategoryDto>.Success(dto);
    }
}