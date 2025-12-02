using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CleanCCM.Application.Features.Products.Queries;

// Nhận nhiều keys: category / tag / ward


public record GetSummaryProductQuery(
    List<string>? CategoryKeys,
    List<string>? TagKeys,
    List<string>? WardKeys
) : IRequest<Result<ProductSummaryDto>>;



public class GetSummaryProductQueryHandler
    : IRequestHandler<GetSummaryProductQuery, Result<ProductSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSummaryProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductSummaryDto>> Handle(
        GetSummaryProductQuery request,
        CancellationToken cancellationToken)
    {
        var categoryKeys = Normalize(request.CategoryKeys);
        var tagKeys = Normalize(request.TagKeys);
        var wardKeys = Normalize(request.WardKeys);

        // ============================
        // 1. CATEGORY SUMMARY (GLOBAL – KHÔNG ĐỔI)
        // ============================
        var categories = await _context.Categories.ToListAsync(cancellationToken);

        var catCountsRaw = await _context.ProductCategories
            .GroupBy(pc => pc.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Count = g.Select(x => x.ProductId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        var catCountDict = catCountsRaw.ToDictionary(x => x.CategoryId, x => x.Count);

        var categorySummaries = categories
            .Select(c => new CategorySummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ProductCount = catCountDict.TryGetValue(c.Id, out var cnt) ? cnt : 0
            })
            .OrderByDescending(c => c.ProductCount)
            .ToList();

        // ============================
        // 2. TAG SUMMARY (PHỤ THUỘC CategoryKeys)
        // ============================

        // Base query cho tầng Tag: PHỤ THUỘC CategoryKeys
        var tagBaseProductQuery = _context.Products.AsQueryable();

        if (categoryKeys is { Count: > 0 })
        {
            tagBaseProductQuery = tagBaseProductQuery.Where(p =>
                p.ProductCategories.Any(pc =>
                    pc.Category.Slug != null &&
                    categoryKeys.Contains(pc.Category.Slug)));
        }

        // Lấy list ProductId đang xét cho tầng Tag
        var filteredProductIdsForTags = await tagBaseProductQuery
            .Select(p => p.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        var tags = await _context.Tags.ToListAsync(cancellationToken);
        List<TagSummaryDto> tagSummaries;

        if (filteredProductIdsForTags.Count == 0)
        {
            // Không có sản phẩm nào trong category vừa chọn → tất cả tag = 0
            tagSummaries = tags
                .Select(t => new TagSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    ProductCount = 0
                })
                .OrderByDescending(t => t.ProductCount)
                .ToList();
        }
        else
        {
            // ĐẾM TAG CHỈ TRÊN TẬP PRODUCT ĐÃ LỌC THEO CATEGORY
            var tagCountsRaw = await _context.ProductTags
                .Where(pt => filteredProductIdsForTags.Contains(pt.ProductId))
                .GroupBy(pt => pt.TagId)
                .Select(g => new
                {
                    TagId = g.Key,
                    Count = g.Select(x => x.ProductId).Distinct().Count()
                })
                .ToListAsync(cancellationToken);

            var tagCountDict = tagCountsRaw.ToDictionary(x => x.TagId, x => x.Count);

            tagSummaries = tags
                .Select(t => new TagSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    ProductCount = tagCountDict.TryGetValue(t.Id, out var cnt) ? cnt : 0
                })
                .OrderByDescending(t => t.ProductCount)
                .ToList();
        }

        // ============================
        // 3. WARD SUMMARY (PHỤ THUỘC CategoryKeys + TagKeys)
        // ============================

        var wardBaseProductQuery = _context.Products.AsQueryable();

        if (categoryKeys is { Count: > 0 })
        {
            wardBaseProductQuery = wardBaseProductQuery.Where(p =>
                p.ProductCategories.Any(pc =>
                    pc.Category.Slug != null &&
                    categoryKeys.Contains(pc.Category.Slug)));
        }

        if (tagKeys is { Count: > 0 })
        {
            wardBaseProductQuery = wardBaseProductQuery.Where(p =>
                p.ProductTags.Any(pt =>
                    pt.Tag.Slug != null &&
                    tagKeys.Contains(pt.Tag.Slug)));
        }

        var filteredProductIdsForWards = await wardBaseProductQuery
            .Select(p => p.Id)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Toàn bộ Ward có trong hệ thống
        var allWardKeys = await _context.Addresses
            .Where(a => a.Ward != null)
            .Select(a => a.Ward!)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<WardSummaryDto> wardSummaries;

        if (filteredProductIdsForWards.Count == 0)
        {
            wardSummaries = allWardKeys
                .Select(w => new WardSummaryDto
                {
                    WardKey = w,
                    ProductCount = 0
                })
                .OrderByDescending(w => w.ProductCount)
                .ToList();
        }
        else
        {
            var wardCountsRaw = await _context.Addresses
                .Where(a => a.Ward != null && filteredProductIdsForWards.Contains(a.ProductId.Value))
                .GroupBy(a => a.Ward)
                .Select(g => new
                {
                    Ward = g.Key!,
                    Count = g.Select(x => x.ProductId).Distinct().Count()
                })
                .ToListAsync(cancellationToken);

            var wardCountDict = wardCountsRaw.ToDictionary(x => x.Ward, x => x.Count);

            wardSummaries = allWardKeys
                .Select(w => new WardSummaryDto
                {
                    WardKey = w,
                    ProductCount = wardCountDict.TryGetValue(w, out var cnt) ? cnt : 0
                })
                .OrderByDescending(w => w.ProductCount)
                .ToList();
        }

        // ============================
        // 4. TotalProducts = tập đã lọc theo Category + Tag + Ward (nếu bạn cần)
        // ============================
        var totalBaseQuery = _context.Products.AsQueryable();

        if (categoryKeys is { Count: > 0 })
        {
            totalBaseQuery = totalBaseQuery.Where(p =>
                p.ProductCategories.Any(pc =>
                    pc.Category.Slug != null &&
                    categoryKeys.Contains(pc.Category.Slug)));
        }

        if (tagKeys is { Count: > 0 })
        {
            totalBaseQuery = totalBaseQuery.Where(p =>
                p.ProductTags.Any(pt =>
                    pt.Tag.Slug != null &&
                    tagKeys.Contains(pt.Tag.Slug)));
        }

        if (wardKeys is { Count: > 0 })
        {
            totalBaseQuery = totalBaseQuery.Where(p =>
                p.Addresses.Any(a =>
                    a.Ward != null &&
                    wardKeys.Contains(a.Ward)));
        }

        var totalProducts = await totalBaseQuery
            .Select(p => p.Id)
            .Distinct()
            .CountAsync(cancellationToken);

        var summary = new ProductSummaryDto
        {
            TotalProducts = totalProducts,
            Categories = categorySummaries,
            Tags = tagSummaries,
            Wards = wardSummaries
        };

        return Result<ProductSummaryDto>.Success(summary);
    }

    private static List<string>? Normalize(List<string>? list)
    {
        return list?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}