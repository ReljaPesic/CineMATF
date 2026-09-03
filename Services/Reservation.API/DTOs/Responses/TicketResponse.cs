namespace Reservation.API.DTOs.Responses;

public record TicketResponse(
    Guid Id,
    Guid ReservationId,
    Guid SeatId,
    int SeatRow,
    int SeatNumber,
    decimal Price,
    string QrCode
);
