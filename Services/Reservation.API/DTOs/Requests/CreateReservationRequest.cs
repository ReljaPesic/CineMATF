using System.ComponentModel.DataAnnotations;

namespace Reservation.API.DTOs.Requests;

public record CreateReservationRequest(
    [Required] Guid ScreeningId,
    [Required, MinLength(1)] List<Guid> SeatIds,
    [Required] Guid UserId
);
