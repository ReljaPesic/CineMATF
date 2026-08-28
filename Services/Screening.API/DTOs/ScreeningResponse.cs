using Screening.API.Entities;

namespace Screening.API.DTOs;

public record ScreeningResponse(
    Guid Id,
    Guid MovieId,
    Guid HallId,
    Guid CinemaId,
    DateTime StartTime,
    ScreeningFormat Format,
    ScreeningStatus Status
);
