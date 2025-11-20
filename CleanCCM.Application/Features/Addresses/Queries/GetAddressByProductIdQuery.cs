using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Application.Addresses.DTOs;

namespace CleanCCM.Application.Features.Addresses.Commands;

public record GetAddressByProductIdQuery(Guid ProductId)
    : IRequest<Result<AddressDto?>>;

public class GetAddressByProductQueryHandler
    : IRequestHandler<GetAddressByProductIdQuery, Result<AddressDto?>>
{
    private readonly IAddressRepository _repository;

    public GetAddressByProductQueryHandler(IAddressRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AddressDto?>> Handle(
        GetAddressByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
        {
            return Result<AddressDto?>.Failure(
                Error.Validation(BaseErrors.NotFoundById, "ProductId is required"));
        }

        var address = await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (address == null)
            return Result<AddressDto?>.Success(null);

        var dto = new AddressDto
        {
            Id = address.Id,
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            District = address.District,
            Ward = address.Ward,
            Country = address.Country,
            IsPrimary = address.IsPrimary,
            ProductId = address.ProductId,
            UserId = address.UserId
        };

        return Result<AddressDto?>.Success(dto);
    }
}
