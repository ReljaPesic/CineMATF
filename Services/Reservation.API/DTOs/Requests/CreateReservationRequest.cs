using System.ComponentModel.DataAnnotations;

namespace Reservation.API.DTOs.Requests;

public record CreateReservationRequest(
    [Required] Guid ScreeningId,
    [Required] IEnumerable<Guid> SeatIds,
    [Required] Guid UserId
);
