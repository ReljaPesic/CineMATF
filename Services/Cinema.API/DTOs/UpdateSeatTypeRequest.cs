using System.ComponentModel.DataAnnotations;

namespace Cinema.API.DTOs;

public record UpdateSeatTypeRequest(
    [Required] string SeatType
);
