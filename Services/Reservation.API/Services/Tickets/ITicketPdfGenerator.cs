using Reservation.API.ExternalServices;
using Entities = Reservation.API.Domain.Entities;

namespace Reservation.API.Services.Tickets;

public interface ITicketPdfGenerator
{
    byte[] Generate(Entities.Ticket ticket, ScreeningDetails screening, CinemaDetails? cinema, MovieDetails? movie);
}
