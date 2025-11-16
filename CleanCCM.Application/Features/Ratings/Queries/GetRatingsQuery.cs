using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Ratings.DTOs;

namespace CleanCCM.Application.Features.Ratings.Queries;

public record GetRatingsQuery(Guid ProductId) : IRequest<Result<List<RatingDto>>>;

public class GetRatingsQueryHandler : IRequestHandler<GetRatingsQuery, Result<List<RatingDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRatingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RatingDto>>> Handle(GetRatingsQuery request, CancellationToken cancellationToken)
    {
        var ratings = await _context.Ratings
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Score = r.Score,
                Review = r.Review,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<RatingDto>>.Success(ratings);
    }
}