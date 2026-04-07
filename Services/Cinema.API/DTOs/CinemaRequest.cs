using System.ComponentModel.DataAnnotations;

namespace Cinema.API.DTOs;

public record CinemaRequest(
    [Required, StringLength(100)] string Name,
    [Required, StringLength(100)] string City
);
