using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Tags.DTOs;

namespace CleanCCM.Application.Features.Tags.Queries;

public record GetAllTagsQuery : IRequest<Result<List<TagDto>>>;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<List<TagDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TagDto>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Tags
            .Include(t => t.ProductTags)
            .OrderBy(t => t.Name)
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

        return Result<List<TagDto>>.Success(tags);
    }
}