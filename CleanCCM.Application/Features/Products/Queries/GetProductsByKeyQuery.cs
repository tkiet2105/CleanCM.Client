using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Products.DTOs;
using CleanCCM.Application.Features.Categories.DTOs;
using CleanCCM.Application.Features.Tags.DTOs;


namespace CleanCCM.Application.Features.Products.Queries;


public record GetProductsByKeyQuery(
    List<string>? CategoryKeys,
    List<string>? TagKeys,
    List<string>? WardKeys,
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedList<ProductDto>>>;

public class GetProductsByKeyQueryHandler
    : IRequestHandler<GetProductsByKeyQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ProductDto>>> Handle(
        GetProductsByKeyQuery request,
        CancellationToken cancellationToken)
    {
        // ========================================
        // CHUẨN HOÁ INPUT: Trim, Distinct, Remove empty
        // ========================================
        var categoryKeys = NormalizeKeys(request.CategoryKeys);
        var tagKeys = NormalizeKeys(request.TagKeys);
        var wardKeys = NormalizeKeys(request.WardKeys);

        // ========================================
        // BASE QUERY: Chỉ lấy sản phẩm đã publish
        // ========================================
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.Addresses)
            .Where(p => p.IsPublished);

        // ========================================
        // FILTER THEO CATEGORY
        // Nếu categoryKeys NULL hoặc EMPTY → Lấy tất cả (không filter)
        // Nếu categoryKeys có giá trị → Chỉ lấy product thuộc các category đó
        // ========================================
        if (HasValues(categoryKeys))
        {
            query = query.Where(p =>
                p.ProductCategories.Any(pc =>
                    pc.Category.Slug != null &&
                    categoryKeys!.Contains(pc.Category.Slug)));
        }

        // ========================================
        // FILTER THEO TAG
        // Nếu tagKeys NULL hoặc EMPTY → Lấy tất cả (không filter)
        // Nếu tagKeys có giá trị → Chỉ lấy product có các tag đó
        // ========================================
        if (HasValues(tagKeys))
        {
            query = query.Where(p =>
                p.ProductTags.Any(pt =>
                    pt.Tag.Slug != null &&
                    tagKeys!.Contains(pt.Tag.Slug)));
        }

        // ========================================
        // FILTER THEO WARD (Khu vực)
        // Nếu wardKeys NULL hoặc EMPTY → Lấy tất cả (không filter)
        // Nếu wardKeys có giá trị → Chỉ lấy product có địa chỉ thuộc các ward đó
        // ========================================
        if (HasValues(wardKeys))
        {
            query = query.Where(p =>
                p.Addresses.Any(a =>
                    !string.IsNullOrEmpty(a.Ward) &&
                    wardKeys!.Contains(a.Ward)));
        }

        // ========================================
        // SORT: Mới nhất trước
        // ========================================
        query = query.OrderByDescending(p => p.CreatedAt);

        // ========================================
        // COUNT: Tổng số record trước khi paging
        // ========================================
        var totalCount = await query.CountAsync(cancellationToken);

        // ========================================
        // PAGING: Skip và Take
        // ========================================
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsPublished = p.IsPublished,
                CreatedAt = p.CreatedAt,
                Categories = p.ProductCategories
                   .Where(pc => pc.Category != null)
                   .Select(pc => new CategoryDto
                   {
                       Id = pc.CategoryId,          // HOẶC pc.Category.Id
                       Name = pc.Category.Name,
                       Slug = pc.Category.Slug,
                       Icon = pc.Category.Icon        // nếu có field Icon
                   })
                   .ToList(),

                Tags = p.ProductTags
                     .Where(pt => pt.Tag != null)
                     .Select(pt => new TagDto
                     {
                         Id = pt.TagId,              // HOẶC pt.Tag.Id
                         Name = pt.Tag.Name,
                         Slug = pt.Tag.Slug,
                         Icon = pt.Tag.Icon            // nếu có
                     })
                     .ToList(),

                ImageUrl = p.Images
                    .Where(i => i.IsPrimary)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.Url)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        // ========================================
        // RESULT: Return paginated list
        // ========================================
        var page = new PaginatedList<ProductDto>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<ProductDto>>.Success(page);
    }

    /// <summary>
    /// Chuẩn hoá danh sách key: Loại bỏ empty, trim, distinct (case-insensitive)
    /// </summary>
    /// <param name="keys">Danh sách key cần chuẩn hoá</param>
    /// <returns>Danh sách đã được chuẩn hoá, hoặc NULL nếu không có giá trị hợp lệ</returns>
    private static List<string>? NormalizeKeys(List<string>? keys)
    {
        if (keys == null || keys.Count == 0)
            return null;

        var normalized = keys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalized.Count > 0 ? normalized : null;
    }

    /// <summary>
    /// Kiểm tra xem list có giá trị hay không
    /// </summary>
    /// <param name="keys">Danh sách cần kiểm tra</param>
    /// <returns>True nếu list có giá trị, False nếu null hoặc empty</returns>
    private static bool HasValues(List<string>? keys)
    {
        return keys is { Count: > 0 };
    }
}