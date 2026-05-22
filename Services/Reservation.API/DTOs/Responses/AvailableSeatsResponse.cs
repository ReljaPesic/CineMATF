namespace Reservation.API.DTOs.Responses;

public record AvailableSeatsResponse(
    Guid ScreeningId,
    IEnumerable<Guid> AvailableSeats,
    IEnumerable<SeatLockResponse> LockedSeats
);
