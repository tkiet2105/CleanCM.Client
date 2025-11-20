using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Products.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Application.Features.Categories.DTOs;
using CleanCCM.Application.Features.Tags.DTOs;

namespace CleanCCM.Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            return Result<ProductDto>.Failure(
                Error.NotFound(BaseErrors.NotFoundById, $"Product {request.Id} not found"));

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price,
            Stock = product.Stock,
            IsPublished = product.IsPublished,
            CreatedAt = product.CreatedAt,
            Categories = product.ProductCategories.Select(pc => new CategoryDto
            {
                Id = pc.Category.Id,
                Name = pc.Category.Name,
                Slug = pc.Category.Slug,
                Icon = pc.Category.Icon
            }).ToList(),
            Tags = product.ProductTags.Select(pt => new TagDto
            {
                Id = pt.Tag.Id,
                Name = pt.Tag.Name,
                Slug = pt.Tag.Slug,
                Color = pt.Tag.Color
            }).ToList()
        };

        return Result<ProductDto>.Success(dto);
    }
}