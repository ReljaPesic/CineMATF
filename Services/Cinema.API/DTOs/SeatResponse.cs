namespace Cinema.API.DTOs;

public record SeatResponse(
    Guid Id,
    int Row,
    int Number,
    string SeatType
);
