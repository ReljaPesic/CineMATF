using Entities = Reservation.API.Domain.Entities;
using Reservation.API.Domain.Enums;

namespace Reservation.API.Services.Pricing;

public interface IReservationFactory
{
    (Entities.Reservation reservation, List<Entities.Ticket> tickets) CreateReservation(
        Guid id, Guid userId, Guid screeningId, ReservationStatus status,
        IEnumerable<Guid> seatIds);
}
