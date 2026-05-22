using AutoMapper;
using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;
using Reservation.API.DTOs.Responses;

namespace Reservation.API.Mapping;

public class ReservationMappingProfile : Profile
{
    public ReservationMappingProfile()
    {
        CreateMap<Entities.Reservation, ReservationResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Entities.SeatLock, SeatLockResponse>();
        CreateMap<Entities.Ticket, TicketResponse>();
    }
}
