using Cinema.API.DTOs;
using Cinema.API.Entities;
using Cinema.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CinemaController(ICinemaRepository repository) : ControllerBase
{
    private readonly ICinemaRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MovieTheatre>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovieTheatre>>> GetCinemas([FromQuery] int page = 1,
                                                                          [FromQuery] int pageSize = 10)
    {
        var (cinemas, _) = await _repository.GetCinemasAsync(page, pageSize);
        return Ok(cinemas);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MovieTheatre), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieTheatre>> GetCinemaById(Guid id)
    {
        var cinema = await _repository.GetCinemaByIdAsync(id);
        if (cinema == null) return NotFound();
        return Ok(cinema);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MovieTheatre), StatusCodes.Status201Created)]
    public async Task<ActionResult<MovieTheatre>> CreateCinema(CreateCinemaRequest request)
    {
        var cinema = await repository.CreateCinemaAsync(request);
        return CreatedAtAction(nameof(GetCinemaById), new { id = cinema.Id }, cinema);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateCinema([FromBody] MovieTheatre cinema)
    {
        var updated = await _repository.UpdateCinemaAsync(cinema);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCinema(Guid id)
    {
        var deleted = await _repository.DeleteCinemaAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }


    [HttpGet("{cinemaId:guid}/halls")]
    [ProducesResponseType(typeof(IEnumerable<Hall>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Hall>>> GetHalls(Guid cinemaId)
    {
        var halls = await _repository.GetHallsAsync(cinemaId);
        return Ok(halls);
    }

    [HttpPost("{cinemaId:guid}/halls")]
    [ProducesResponseType(typeof(Hall), StatusCodes.Status201Created)]
    public async Task<ActionResult<Hall>> CreateHall(Guid cinemaId, [FromBody] Hall hall)
    {
        await _repository.CreateHallAsync(cinemaId, hall);
        return CreatedAtAction(nameof(GetHalls), new { cinemaId }, hall);
    }

    [HttpPut("{cinemaId:guid}/halls/{hallId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHall(Guid cinemaId, Guid hallId, [FromBody] Hall hall)
    {
        if (hallId != hall.Id || hall.CinemaId != cinemaId)
            return BadRequest("ID mismatch");

        var updated = await _repository.UpdateHallAsync(hall);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("{cinemaId:guid}/halls/{hallId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHall(Guid cinemaId, Guid hallId)
    {
        var deleted = await _repository.DeleteHallAsync(cinemaId, hallId);
        if (!deleted) return NotFound();

        return NoContent();
    }

    [HttpGet("halls/{hallId:guid}/seats")]
    [ProducesResponseType(typeof(IEnumerable<Seat>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Seat>>> GetSeats(Guid hallId)
    {
        var seats = await _repository.GetSeatLayoutAsync(hallId);
        return Ok(seats);
    }

    [HttpPost("halls/{hallId:guid}/seats")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSeats(Guid hallId)
    {
        await _repository.CreateSeatsAsync(hallId);
        return StatusCode(StatusCodes.Status201Created);
    }
}
