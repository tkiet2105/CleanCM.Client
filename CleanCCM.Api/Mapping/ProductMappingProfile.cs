using AutoMapper;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.Api.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Nếu GetAllProductQuery có property queryRequest
        CreateMap<GetAllProductQueryRequest, GetAllProductQuery>()
             .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
 
    }
}