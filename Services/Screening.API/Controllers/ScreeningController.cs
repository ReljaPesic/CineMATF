using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Screening.API.Authorization;
using Screening.API.DTOs;
using Screening.API.Services;

namespace Screening.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ScreeningController(IScreeningService service) : ControllerBase
{
    // Public browsing endpoint (no [Authorize]) - but a CinemaAdmin's own screenings are
    // the only ones they should see, so the list is restricted to their cinema(s) when the
    // caller happens to be authenticated as one. Everyone else sees the full public catalog.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScreeningResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ScreeningResponse>>> GetScreenings(
        [FromQuery] Guid? movieId,
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? cinemaId)
    {
        var restrictToCinemaIds = User.IsCinemaAdmin() ? User.GetCinemaIds() : null;
        var screenings = await service.GetScreeningsAsync(movieId, date, cinemaId, restrictToCinemaIds);
        return Ok(screenings);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScreeningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreeningResponse>> GetScreeningById(Guid id)
    {
        var screening = await service.GetScreeningByIdAsync(id);
        if (screening == null) return NotFound();
        return Ok(screening);
    }

    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.CinemaAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(ScreeningResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ScreeningResponse>> CreateScreening([FromBody] ScreeningRequest request)
    {
        if (!User.CanManageCinema(request.CinemaId)) return Forbid();

        var (success, errorMessage, screening) = await service.CreateScreeningAsync(request);
        if (!success) return BadRequest(new { message = errorMessage });

        return CreatedAtAction(nameof(GetScreeningById), new { id = screening!.Id }, screening);
    }

    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.CinemaAdmin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ScreeningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreeningResponse>> UpdateScreening(Guid id, [FromBody] ScreeningRequest request)
    {
        var existing = await service.GetScreeningByIdAsync(id);
        if (existing == null) return NotFound();
        // A CinemaAdmin may only touch screenings in their own cinema, and can't move one out of it.
        if (!User.CanManageCinema(existing.CinemaId) || !User.CanManageCinema(request.CinemaId)) return Forbid();

        var (success, errorMessage, screening) = await service.UpdateScreeningAsync(id, request);
        if (!success)
            return errorMessage == "Screening not found"
                ? NotFound()
                : BadRequest(new { message = errorMessage });

        return Ok(screening);
    }

    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.CinemaAdmin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScreening(Guid id)
    {
        var existing = await service.GetScreeningByIdAsync(id);
        if (existing == null) return NotFound();
        if (!User.CanManageCinema(existing.CinemaId)) return Forbid();

        var deleted = await service.DeleteScreeningAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
