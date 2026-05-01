using Cinema.API.Entities;

namespace Cinema.API.DTOs;

public record CinemaResponse(
    Guid Id,
    string Name,
    City City
);
