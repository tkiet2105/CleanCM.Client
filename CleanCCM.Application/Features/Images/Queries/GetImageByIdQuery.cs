using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Application.Features.Images.DTOs;
using CleanCCM.Application.Features.Tags.DTOs;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Application.Features.Images.Queries;

public record GetImageByIdQuery(Guid Id) : IRequest<Result<ImageDto>>;


public class GetImageByIdQueryHandler
    : IRequestHandler<GetImageByIdQuery, Result<ImageDto>>
{
    private readonly IImageRepository _imageRepository;

    public GetImageByIdQueryHandler(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<Result<ImageDto>> Handle(
        GetImageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var image = await _imageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (image == null)
        {
            return Result<ImageDto>.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Image not found"));
        }

        return Result<ImageDto>.Success(Map(image));
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