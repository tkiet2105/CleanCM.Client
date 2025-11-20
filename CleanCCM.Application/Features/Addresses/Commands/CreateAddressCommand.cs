using MediatR;
using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Entities;

namespace CleanCCM.Application.Features.Addresses.Commands;

public record CreateAddressCommand : IRequest<Result<Guid>>
{
    public string Line1 { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Ward { get; init; } = string.Empty;

    public string? Line2 { get; init; }
    public Guid? ProductId { get; init; }
    public string? UserId { get; init; }
    public bool IsPrimary { get; init; }

    public string Country { get; init; } = "Vietnam";
}

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, Result<Guid>>
{
    private readonly IRepository<Address> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAddressCommandHandler(
        IRepository<Address> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var address = Address.Create(
             line1: request.Line1,
             city: request.City,
             district: request.District,
             ward: request.Ward,
             line2: request.Line2,
             productId: request.ProductId,
             userId: request.UserId,
             isPrimary: request.IsPrimary,
             country: request.Country
         );

        await _repository.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(address.Id);
    }
}