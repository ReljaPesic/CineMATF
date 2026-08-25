namespace Reservation.API.DTOs.Responses;

public record TicketResponse(
    Guid Id,
    Guid SeatId,
    int SeatRow,
    int SeatNumber,
    decimal Price,
    string QrCode
);
