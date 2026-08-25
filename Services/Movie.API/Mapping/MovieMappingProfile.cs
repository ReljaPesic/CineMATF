using AutoMapper;
using Movie.API.DTOs;

namespace Movie.API.Mapping;

public class MovieMappingProfile : Profile
{
    public MovieMappingProfile()
    {
        CreateMap<Entities.Movie, MovieResponse>();
        CreateMap<MovieRequest, Entities.Movie>();
    }
}
