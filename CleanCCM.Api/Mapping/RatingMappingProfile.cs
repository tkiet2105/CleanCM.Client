using AutoMapper;
using CleanCCM.Application.Features.Ratings.Commands;
using CleanCCM.Application.Features.Ratings.Queries;
using CleanCCM.Shared.Rating.Requests;
using CleanCCM.Shared.Rating.Responses;

namespace CleanCCM.Api.Mappings;

public class RatingMappingProfile : Profile
{
    public RatingMappingProfile()
    {
        CreateMap<CreateRatingRequest, CreateRatingCommand>();
        CreateMap<UpdateRatingRequest, UpdateRatingCommand>();
        CreateMap<GetRatingsByProductRequest, GetRatingsByProductIdQuery>();
    }
}
