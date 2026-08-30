using System.ComponentModel.DataAnnotations;
using Cinema.API.Entities;

namespace Cinema.API.DTOs;

public record UpdateSeatTypeRequest(
    [Required] SeatType SeatType
);
