using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using MediatR;

namespace CleanCCM.Application.Features.Images.Commands;

public record CreateImageCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;

    public string? Alt { get; init; }
    public int SortOrder { get; init; } = 0;
    public bool IsPrimary { get; init; } = false;
}

public class CreateImageCommandHandler
    : IRequestHandler<CreateImageCommand, Result<Guid>>
{
    private readonly IImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateImageCommandHandler(
        IImageRepository imageRepository,
        IUnitOfWork unitOfWork)
    {
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateImageCommand request,
        CancellationToken cancellationToken)
    {
        var image = Image.Create(
            productId: request.ProductId,
            fileName: request.FileName,
            url: request.Url,
            alt: request.Alt,
            sortOrder: request.SortOrder,
            isPrimary: request.IsPrimary
        );

        await _imageRepository.AddAsync(image, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(image.Id);
    }
}