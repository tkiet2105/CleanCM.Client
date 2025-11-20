using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Tags.DTOs;
using CleanCCM.Application.Features.Tags.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Tags.Queries;

public record GetAllTagsQuery : IRequest<Result<List<TagDto>>>;


public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<List<TagDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<Result<List<TagDto>>> Handle(
        GetAllTagsQuery request,
        CancellationToken cancellationToken)
    {
        // Base query
        var tags = await _context.Tags
        .Include(c => c.ProductTags)
        .OrderBy(c => c.Name)
        .Select(c => new TagDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            Slug = c.Slug,
            CreatedAt = c.CreatedAt
        })
        .ToListAsync(cancellationToken);

        return Result<List<TagDto>>.Success(tags);
    }
}