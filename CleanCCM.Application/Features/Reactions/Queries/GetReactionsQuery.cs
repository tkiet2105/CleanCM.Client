using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Reactions.DTOs;

namespace CleanCCM.Application.Features.Reactions.Queries;

public record GetReactionsQuery(Guid ProductId) : IRequest<Result<ReactionDto>>;

public class GetReactionsQueryHandler : IRequestHandler<GetReactionsQuery, Result<ReactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReactionDto>> Handle(GetReactionsQuery request, CancellationToken cancellationToken)
    {
        var reactions = await _context.Reactions
            .Where(r => r.ProductId == request.ProductId)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var dto = new ReactionDto
        {
            ProductId = request.ProductId,
            TotalCount = reactions.Sum(r => r.Count),
            LikeCount = reactions.FirstOrDefault(r => r.Type == Domain.Entities.ReactionType.Like)?.Count ?? 0,
            LoveCount = reactions.FirstOrDefault(r => r.Type == Domain.Entities.ReactionType.Love)?.Count ?? 0,
            HahaCount = reactions.FirstOrDefault(r => r.Type == Domain.Entities.ReactionType.Haha)?.Count ?? 0,
            WowCount = reactions.FirstOrDefault(r => r.Type == Domain.Entities.ReactionType.Wow)?.Count ?? 0,
            SadCount = reactions.FirstOrDefault(r => r.Type == Domain.Entities.ReactionType.Sad)?.Count ?? 0,
            AngryCount = reactions.FirstOrDefault(r => r.Type == Domain.Entities.ReactionType.Angry)?.Count ?? 0
        };

        return Result<ReactionDto>.Success(dto);
    }
}