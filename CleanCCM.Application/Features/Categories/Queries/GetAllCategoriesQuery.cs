using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Categories.DTOs;
using CleanCCM.Application.Features.Tags.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CleanCCM.Application.Features.Categories.Queries;

public record GetAllCategoriesQuery : IRequest<Result<List<CategoryDto>>>;

public class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryDto>>> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Có thể dùng GetByDisplayOrderAsync để mặc định sort
        var categories = await _categoryRepository.GetByDisplayOrderAsync(cancellationToken);

        var list = categories
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