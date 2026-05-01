using System.ComponentModel.DataAnnotations;

namespace Cinema.API.DTOs;

public record HallRequest(
    [Required, StringLength(50)] string Name,
    [Range(1, 100)] int TotalRows,
    [Range(1, 50)] int SeatsPerRow
);
