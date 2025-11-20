using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Images.DTOs;
using CleanCCM.Application.Features.Tags.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Tags.Queries;

public record GetPrimaryImageQuery(Guid ProductId)
    : IRequest<Result<ImageDto?>>;
public class GetPrimaryImageQueryHandler
    : IRequestHandler<GetPrimaryImageQuery, Result<ImageDto?>>
{
    private readonly IImageRepository _imageRepository;

    public GetPrimaryImageQueryHandler(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<Result<ImageDto?>> Handle(
        GetPrimaryImageQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<ImageDto?>.Failure(
                Error.Validation(BaseErrors.NotFoundById, "ProductId is required"));
        }

        var image = await _imageRepository.GetPrimaryImageAsync(request.ProductId, cancellationToken);

        if (image == null)
            return Result<ImageDto?>.Success(null);

        return Result<ImageDto?>.Success(Map(image));
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