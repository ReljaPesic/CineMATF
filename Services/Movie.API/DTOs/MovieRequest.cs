using System.ComponentModel.DataAnnotations;
using Movie.API.Entities;

namespace Movie.API.DTOs;

public record MovieRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(2000)] string Description,
    [Range(1, 1000)] int DurationMinutes,
    DateTime ReleaseDate,
    [Range(0, 10)] double Rating,
    [Required] List<Actor> Actors,
    [Required, MinLength(1)] List<Genre> Genres,
    string? CoverImage
);
