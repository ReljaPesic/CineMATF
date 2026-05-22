namespace Reservation.API.DTOs.Responses;

public record SeatLockResponse(
    Guid SeatId,
    DateTime LockedAt,
    DateTime ExpiresAt
);
