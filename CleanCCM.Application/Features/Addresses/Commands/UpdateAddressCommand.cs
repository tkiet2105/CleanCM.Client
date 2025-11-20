using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;
using CleanCCM.Domain.Common.Errors;

namespace CleanCCM.Application.Features.Addresses.Commands;

public record UpdateAddressCommand : IRequest<Result>
{
    public Guid Id { get; init; }

    public string Line1 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Ward { get; init; } = string.Empty;

    public string? Line2 { get; init; }
    public string Country { get; init; } = "Vietnam";

    public bool IsPrimary { get; init; } // là địa chỉ chính

    // Nếu muốn cho phép đổi liên kết
    public Guid? ProductId { get; init; }
    public string? UserId { get; init; }
}

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result>
{
    private readonly IRepository<Address> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAddressCommandHandler(
        IRepository<Address> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (address == null)
        {
            return Result.Failure(
                Error.NotFound(BaseErrors.NotFoundById, "Address not found"));
        }

        // Cập nhật phần chi tiết địa chỉ
        address.UpdateDetails(
            line1: request.Line1,
            city: request.City,
            district: request.District,
            ward: request.Ward,
            line2: request.Line2,
            country: request.Country
        );

        // Cập nhật FK nếu bạn cho phép
        address.AssignToProduct(request.ProductId);
        address.AssignToUser(request.UserId);

        // Cập nhật trạng thái primary
        if (request.IsPrimary)
            address.SetPrimary();
        else
            address.UnsetPrimary();

        _repository.Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
