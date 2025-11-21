using AutoMapper;
using CleanCCM.Application.Features.Tags.Commands;
using CleanCCM.Shared.Tags.Requests;

namespace CleanCCM.Api.Mapping;

public class TagMappingProfile : Profile
{
    public TagMappingProfile()
    {
        CreateMap<CreateTagRequest, CreateTagCommand>();
        CreateMap<UpdateTagRequest, UpdateTagCommand>();
    }
}