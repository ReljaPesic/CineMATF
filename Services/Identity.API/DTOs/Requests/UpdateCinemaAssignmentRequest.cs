using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs.Requests;

// Body of PUT /api/v1/User/{username}/cinemas - SuperAdmin reassigning an
// existing CinemaAdmin's cinema(s).
public record UpdateCinemaAssignmentRequest
{
    [Required(ErrorMessage = "At least one CinemaId is required")]
    [MinLength(1, ErrorMessage = "At least one CinemaId is required")]
    public List<Guid> CinemaIds { get; set; } = new();
}
