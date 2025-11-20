using AutoMapper;
using CleanCCM.Application.Features.Products.Commands;
using CleanCCM.Application.Features.Products.Queries;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.Api.Mapping;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<GetAllProductRequest, GetAllProductQuery>();

        CreateMap<CreateProductRequest, CreateProductCommand>();

        CreateMap<UpdateProductRequest, UpdateProductCommand>();

    }
}