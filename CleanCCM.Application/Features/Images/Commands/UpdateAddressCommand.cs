using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Images.Commands;

public record UpdateImageCommand : IRequest<Result>
{
    public Guid Id { get; init; }

    // Metadata
    public string? Alt { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsPrimary { get; init; }

    // Nếu có cập nhật lại file
    public string? FileName { get; init; }
    public string? Url { get; init; }

    // Nếu muốn cho phép đổi product
    public Guid? ProductId { get; init; }
}
public class UpdateImageCommandHandler : IRequestHandler<UpdateImageCommand, Result>
{
    private readonly IImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateImageCommandHandler(
        IImageRepository imageRepository,
        IUnitOfWork unitOfWork)
    {
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _imageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (image == null)
        {
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Image not found"));
        }

        // Nếu có yêu cầu đổi file, bắt buộc phải đủ cả FileName + Url
        if (request.FileName is not null || request.Url is not null)
        {
            if (string.IsNullOrWhiteSpace(request.FileName) ||
                string.IsNullOrWhiteSpace(request.Url))
            {
                return Result.Failure(
                    Error.Validation(BaseErrors.InvalidUrl, "FileName and Url are required when updating file"));
            }

            image.UpdateFile(request.FileName!, request.Url!);
        }

        // Update metadata (alt, sortOrder, isPrimary)
        image.UpdateInfo(
            alt: request.Alt,
            sortOrder: request.SortOrder,
            isPrimary: request.IsPrimary
        );

        // Đổi product nếu có
        if (request.ProductId.HasValue && request.ProductId.Value != Guid.Empty)
        {
            image.MoveToProduct(request.ProductId.Value);
        }

        _imageRepository.Update(image);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}