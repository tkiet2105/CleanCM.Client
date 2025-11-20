using CleanCCM.Application.Addresses.DTOs;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common.Errors;
using MediatR;

namespace CleanCCM.Application.Features.Addresses.Commands;

public record GetAddressByUserIdQuery(string UserId)
    : IRequest<Result<List<AddressDto>>>;

public class GetAddressByUserIdQueryHandler
    : IRequestHandler<GetAddressByUserIdQuery, Result<List<AddressDto>>>
{
    private readonly IAddressRepository _addressRepository;

    public GetAddressByUserIdQueryHandler(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<Result<List<AddressDto>>> Handle(
        GetAddressByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result<List<AddressDto>>.Failure(
                Error.Validation(BaseErrors.NotFoundById, "UserId is required"));
        }

        var addresses = await _addressRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var data = addresses
            .Select(a => new AddressDto
            {
                Id = a.Id,
                Line1 = a.Line1,
                Line2 = a.Line2,
                City = a.City,
                District = a.District,
                Ward = a.Ward,
                Country = a.Country,
                IsPrimary = a.IsPrimary,
                ProductId = a.ProductId,
                UserId = a.UserId
            })
            .ToList();

        return Result<List<AddressDto>>.Success(data);
    }
}
