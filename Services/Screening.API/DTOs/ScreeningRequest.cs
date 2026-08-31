using System.ComponentModel.DataAnnotations;
using Screening.API.Entities;

namespace Screening.API.DTOs;

public record ScreeningRequest(
    [Required] Guid MovieId,
    [Required] Guid HallId,
    [Required] Guid CinemaId,
    [Required] DateTime StartTime,
    [Required] ScreeningFormat Format
);
