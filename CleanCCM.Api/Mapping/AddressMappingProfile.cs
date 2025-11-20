using AutoMapper;
using CleanCCM.Application.Features.Addresses.Commands;
using CleanCCM.Shared.Addresses.Requests;

namespace CleanCCM.Api.Mapping;

public class AddressMappingProfile : Profile
{
    public AddressMappingProfile()
    {
        CreateMap<CreateAddressRequest, CreateAddressCommand>();

        CreateMap<UpdateAddressRequest, UpdateAddressCommand>();
    }
}