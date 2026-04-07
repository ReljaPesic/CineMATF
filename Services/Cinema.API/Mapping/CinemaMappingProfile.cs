using AutoMapper;
using Cinema.API.DTOs;
using Cinema.API.Entities;
namespace Cinema.API.Mapping;

public class CinemaMappingProfile : Profile
{
    public CinemaMappingProfile()
    {
        CreateMap<MovieTheatre, CinemaResponse>();
        CreateMap<CinemaRequest, MovieTheatre>();
    }
}
