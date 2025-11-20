using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Products.DTOs;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CleanCCM.Application.Features.Products.Queries;

public record GetAllProductQuery : IRequest<Result<PaginatedList<ProductDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? TagId { get; init; }
}
public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IProductRepository _productRepository;

    public GetAllProductQueryHandler(IApplicationDbContext context, IProductRepository productRepository)
    {
        _context = context;
        _productRepository = productRepository;
    }
    /// <summary>
    /// Cần thiết lập riêng từ DbContext - những cái đơn giản thì không cần , chỉ lấy trực tiếp qua Repository
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm) || p.Description.Contains(request.SearchTerm));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == request.CategoryId.Value));
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(p => p.ProductTags.Any(pt => pt.TagId == request.TagId.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                Stock = p.Stock,
                IsPublished = p.IsPublished,
                CreatedAt = p.CreatedAt,
                ImageUrl = p.Images
                     .Where(i => i.IsPrimary)
                     .OrderBy(i => i.SortOrder)
                     .Select(i => i.Url)
                     .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedList<ProductDto>(products, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<ProductDto>>.Success(paginatedList);
    }
}


