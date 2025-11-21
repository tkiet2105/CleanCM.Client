using AutoMapper;
using CleanCCM.Application.Features.Comments.Commands;
using CleanCCM.Application.Features.Comments.Queries;
using CleanCCM.Shared.Addresses.Requests;
using CleanCCM.Shared.Comments.Requests;

namespace CleanCCM.Api.Mapping;


public class CommentMappingProfile : Profile
{
    public CommentMappingProfile()
    {
        CreateMap<CreateCommentRequest, CreateCommentCommand>();
        CreateMap<UpdateCommentRequest, UpdateCommentCommand>();

        CreateMap<GetCommentsByProductRequest, GetCommentsByProductIdQuery>();
    }
}