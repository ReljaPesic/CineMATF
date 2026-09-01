using AutoMapper;
using Identity.API.DTOs;
using Identity.API.DTOs.Requests;
using Identity.API.Entities;

namespace Identity.API.Mapping;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<RegisterRequest, User>().ReverseMap();
        CreateMap<User, UserDetails>().ReverseMap();
    }
}
