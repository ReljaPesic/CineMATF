using AutoMapper;
using Cinema.API.DTOs;
using Cinema.API.Entities;
namespace Cinema.API.Mapping;

public class HallMappingProfile : Profile
{
    public HallMappingProfile()
    {
        CreateMap<Hall, HallResponse>();
        CreateMap<HallRequest, Hall>();
    }
}
