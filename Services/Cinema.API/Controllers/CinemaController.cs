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
    [ProducesResponseType(typeof(PagedResponse<CinemaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CinemaResponse>>> GetCinemas([FromQuery] int page = 1,
                                                                           [FromQuery] int pageSize = 10)
    {
        var response = await service.GetCinemasAsync(page, pageSize);
        return Ok(response);
    }

    [HttpGet("city/{cityName}")]
    [ProducesResponseType(typeof(IEnumerable<CinemaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CinemaResponse>>> GetCinemasByCity(string cityName)
    {
        var normalizedCity = cityName.Replace(" ", "");
        if (!Enum.TryParse<City>(normalizedCity, ignoreCase: true, out var city))
        {
            return BadRequest(new { message = $"Invalid city name. Valid values: {string.Join(", ", Enum.GetNames<City>())}" });
        }

        var cinemas = await service.GetCinemasByCityAsync(city);
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

    [HttpPost("{cinemaId:guid}/halls")]
    [ProducesResponseType(typeof(CreateHallsResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateHallsResponse>> CreateHalls(Guid cinemaId, [FromBody] CreateHallsRequest request)
    {
        var result = await service.CreateHallsAsync(cinemaId, request.Halls);
        return CreatedAtAction(nameof(GetHalls), new { cinemaId }, result);
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

    [HttpGet("{cinemaId:guid}/halls/{hallId:guid}/seats")]
    [ProducesResponseType(typeof(IEnumerable<SeatResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SeatResponse>>> GetSeats(Guid cinemaId, Guid hallId)
    {
        var seats = await service.GetSeatsAsync(cinemaId, hallId);
        return Ok(seats);
    }

    [HttpPatch("{cinemaId:guid}/halls/{hallId:guid}/seats/{seatId:guid}")]
    [ProducesResponseType(typeof(SeatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeatResponse>> UpdateSeatType(Guid cinemaId, Guid hallId, Guid seatId, [FromBody] UpdateSeatTypeRequest request)
    {
        var seat = await service.UpdateSeatTypeAsync(cinemaId, hallId, seatId, request);
        return Ok(seat);
    }

    [HttpPost("{cinemaId:guid}/halls/{hallId:guid}/seats")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSeats(Guid cinemaId, Guid hallId)
    {
        await service.CreateSeatsAsync(cinemaId, hallId);
        return CreatedAtAction(nameof(GetSeats), new { cinemaId, hallId }, new { message = "Seats created successfully" });
    }
}
