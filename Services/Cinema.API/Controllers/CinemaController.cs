using Cinema.API.DTOs;
using Cinema.API.Entities;
using Cinema.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CinemaController(ICinemaService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CinemaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CinemaResponse>>> GetCinemas([FromQuery] int page = 1,
                                                                           [FromQuery] int pageSize = 10)
    {
        var (cinemas, _) = await service.GetCinemasAsync(page, pageSize);
        return Ok(cinemas);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CinemaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CinemaResponse>> GetCinemaById(Guid id)
    {
        var cinema = await service.GetCinemaByIdAsync(id);
        if (cinema == null) return NotFound();
        return Ok(cinema);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CinemaResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CinemaResponse>> CreateCinema([FromBody] CinemaRequest request)
    {
        var cinema = await service.CreateCinemaAsync(request);
        return CreatedAtAction(nameof(GetCinemaById), new { id = cinema.Id }, cinema);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CinemaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CinemaResponse>> UpdateCinema(Guid id, [FromBody] CinemaRequest request)
    {
        var cinema = await service.UpdateCinemaAsync(id, request);
        if (cinema == null) return NotFound();
        return Ok(cinema);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCinema(Guid id)
    {
        var deleted = await service.DeleteCinemaAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{cinemaId:guid}/halls")]
    [ProducesResponseType(typeof(IEnumerable<HallResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HallResponse>>> GetHalls(Guid cinemaId)
    {
        var halls = await service.GetHallsAsync(cinemaId);
        return Ok(halls);
    }

    [HttpPost("{cinemaId:guid}/hall")]
    [ProducesResponseType(typeof(HallResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<HallResponse>> CreateHall(Guid cinemaId, [FromBody] HallRequest request)
    {
        var hall = await service.CreateHallAsync(cinemaId, request);
        return CreatedAtAction(nameof(GetHalls), new { cinemaId }, hall);
    }

    [HttpPost("{cinemaId:guid}/halls")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateHalls(Guid cinemaId, [FromBody] CreateHallsRequest request)
    {
        var count = await service.CreateHallsAsync(cinemaId, request.Halls);
        return Created($"/api/v1/cinema/{cinemaId}/halls", new { created = count });
    }

    [HttpDelete("{cinemaId:guid}/halls/{hallId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHall(Guid cinemaId, Guid hallId)
    {
        var deleted = await service.DeleteHallAsync(cinemaId, hallId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("halls/{hallId:guid}/seats")]
    [ProducesResponseType(typeof(IEnumerable<Seat>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Seat>>> GetSeats(Guid hallId)
    {
        var seats = await service.GetSeatsAsync(hallId);
        return Ok(seats);
    }

    [HttpPost("halls/{hallId:guid}/seats")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSeats(Guid hallId)
    {
        await service.CreateSeatsAsync(hallId);
        return StatusCode(StatusCodes.Status201Created);
    }
}
