using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Categories.DTOs;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Categories.Queries;

public record GetActiveCategoriesQuery : IRequest<Result<List<CategoryDto>>>;

public class GetActiveCategoriesQueryHandler
    : IRequestHandler<GetActiveCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetActiveCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetActiveCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync(cancellationToken);

        var list = categories
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                Icon = c.Icon,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                ProductCount = c.ProductCategories?.Count ?? 0
            })
            .ToList();

        return Result<List<CategoryDto>>.Success(list);
    }
}
