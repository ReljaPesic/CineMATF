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
    public async Task<ActionResult<IEnumerable<MovieTheatre>>> GetCinemas()
    {
        var cinemas = await _repository.GetCinemasAsync();
        return Ok(cinemas);
    }
}
