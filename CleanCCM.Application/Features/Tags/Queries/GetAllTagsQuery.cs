using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Tags.DTOs;

namespace CleanCCM.Application.Features.Tags.Queries;

public class GetAllTagsQuery
    : PaginationRequest, IRequest<Result<PaginatedList<TagDto>>>
{
}

public class GetAllTagsQueryHandler
    : IRequestHandler<GetAllTagsQuery, Result<PaginatedList<TagDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<TagDto>>> Handle(
        GetAllTagsQuery request,
        CancellationToken cancellationToken)
    {
        // Base query
        var query = _context.Tags
            .AsNoTracking()
            .Include(t => t.ProductTags)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(t =>
                t.Name.Contains(term) ||
                t.Slug.Contains(term));
        }

        // Sort
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sort = request.SortBy.Trim().ToLower();

            query = (sort, request.SortDescending) switch
            {
                ("name", false) => query.OrderBy(t => t.Name),
                ("name", true) => query.OrderByDescending(t => t.Name),
                ("createdat", false) => query.OrderBy(t => t.CreatedAt),
                ("createdat", true) => query.OrderByDescending(t => t.CreatedAt),
                ("productcount", false) => query.OrderBy(t => t.ProductTags.Count),
                ("productcount", true) => query.OrderByDescending(t => t.ProductTags.Count),
                _ => query.OrderBy(t => t.Name) // default
            };
        }
        else
        {
            // Default order
            query = query.OrderBy(t => t.Name);
        }

        // Đếm tổng
        var totalCount = await query.CountAsync(cancellationToken);

        // Lấy page hiện tại
        var items = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Color = t.Color,
                ProductCount = t.ProductTags.Count,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Tạo PaginatedList
        var pagedResult = new PaginatedList<TagDto>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize
        ); 

        // Bọc trong Result<T>
        return Result<PaginatedList<TagDto>>.Success(pagedResult); 
    }
}