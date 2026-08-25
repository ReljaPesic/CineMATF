namespace Reservation.API.DTOs.Responses;

public record ReservationResponse(
    Guid Id,
    Guid UserId,
    Guid ScreeningId,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    IEnumerable<TicketResponse> Tickets
);
