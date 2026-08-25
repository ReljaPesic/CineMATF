using System.ComponentModel.DataAnnotations;

namespace Reservation.API.DTOs.Requests;

public record ConfirmReservationRequest(
    [Required] Guid ReservationId,
    [Required] Guid PaymentId
);
