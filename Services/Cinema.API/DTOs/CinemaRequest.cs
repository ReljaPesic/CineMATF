using System.ComponentModel.DataAnnotations;
using Cinema.API.Entities;

namespace Cinema.API.DTOs;

public record CinemaRequest(
    [Required, StringLength(100)] string Name,
    [Required] City City
);
