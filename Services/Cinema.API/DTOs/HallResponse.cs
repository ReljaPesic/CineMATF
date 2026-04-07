namespace Cinema.API.DTOs;

public record HallResponse(
    Guid Id,
    string Name,
    int TotalRows,
    int SeatsPerRow,
    Guid CinemaId
);
