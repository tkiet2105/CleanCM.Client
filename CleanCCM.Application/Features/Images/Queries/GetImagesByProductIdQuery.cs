using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Images.DTOs;
using CleanCCM.Application.Features.Tags.DTOs;
using CleanCCM.Application.Features.Tags.Queries;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Images.Queries;

public record GetImagesByProductIdQuery(Guid ProductId)
    : IRequest<Result<List<ImageDto>>>;


public class GetImagesByProductIdQueryHandler
    : IRequestHandler<GetImagesByProductIdQuery, Result<List<ImageDto>>>
{
    private readonly IImageRepository _imageRepository;

    public GetImagesByProductIdQueryHandler(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<Result<List<ImageDto>>> Handle(
        GetImagesByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<List<ImageDto>>.Failure(
                Error.Validation(BaseErrors.NotFoundById, "ProductId is required"));
        }

        var images = await _imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        var list = images
            .OrderBy(i => i.SortOrder)
            .Select(Map)
            .ToList();

        return Result<List<ImageDto>>.Success(list);
    }

    private static ImageDto Map(Image image) =>
        new()
        {
            Id = image.Id,
            ProductId = image.ProductId,
            FileName = image.FileName,
            Url = image.Url,
            Alt = image.Alt,
            SortOrder = image.SortOrder,
            IsPrimary = image.IsPrimary,
            CreatedAt = image.CreatedAt
        };
}