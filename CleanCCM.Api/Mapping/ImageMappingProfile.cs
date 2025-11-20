using AutoMapper;
using CleanCCM.Application.Features.Images.Commands;
using CleanCCM.Shared.Images.Requests;

namespace CleanCCM.Api.Mapping;

public class ImageMappingProfile : Profile
{
    public ImageMappingProfile()
    {
        CreateMap<CreateImageRequest, CreateImageCommand>();

        CreateMap<UpdateImageRequest, UpdateImageCommand>();
    }
}