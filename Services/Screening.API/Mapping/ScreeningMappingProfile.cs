using AutoMapper;
using Screening.API.DTOs;
using Entities = Screening.API.Entities;

namespace Screening.API.Mapping;

public class ScreeningMappingProfile : Profile
{
    public ScreeningMappingProfile()
    {
        CreateMap<Entities.Screening, ScreeningResponse>();
        CreateMap<ScreeningRequest, Entities.Screening>();
    }
}
