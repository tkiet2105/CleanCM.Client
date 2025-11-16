using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Tags.DTOs;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Tags.Queries;

public record GetTagByIdQuery(Guid Id) : IRequest<Result<TagDto>>;

public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, Result<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTagByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TagDto>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _context.Tags
            .Include(t => t.ProductTags)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag == null)
            return Result<TagDto>.Failure(
                Error.NotFound(BaseErrors.NotFoundById, $"Tag {request.Id} not found"));

        var dto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            Color = tag.Color,
            ProductCount = tag.ProductTags.Count,
            CreatedAt = tag.CreatedAt
        };

        return Result<TagDto>.Success(dto);
    }
}