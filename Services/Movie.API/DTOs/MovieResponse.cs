using Movie.API.Entities;

namespace Movie.API.DTOs;

public record MovieResponse(
    Guid Id,
    string Title,
    string Description,
    int DurationMinutes,
    DateTime ReleaseDate,
    double Rating,
    List<Actor> Actors,
    List<Genre> Genres,
    string? CoverImage
);
