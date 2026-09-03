using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie.API.Authorization;
using Movie.API.DTOs;
using Movie.API.Entities;
using Movie.API.Services;

namespace Movie.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MovieController(IMovieService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MovieResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<MovieResponse>>> GetMovies([FromQuery] int page = 1,
                                                                             [FromQuery] int pageSize = 10)
    {
        var response = await service.GetMoviesAsync(page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> GetMovieById(Guid id)
    {
        var movie = await service.GetMovieByIdAsync(id);
        if (movie == null) return NotFound();
        return Ok(movie);
    }

    [HttpGet("genre/{genre}")]
    [ProducesResponseType(typeof(IEnumerable<MovieResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<MovieResponse>>> GetMoviesByGenre(string genre)
    {
        if (!Enum.TryParse<Genre>(genre, ignoreCase: true, out var parsedGenre))
            return BadRequest(new { message = "Invalid genre." });

        var movies = await service.GetMoviesByGenreAsync(parsedGenre);
        return Ok(movies);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<MovieResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MovieResponse>>> GetMoviesByTitle([FromQuery] string title)
    {
        var movies = await service.GetMoviesByTitleAsync(title);
        return Ok(movies);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<MovieResponse>> CreateMovie([FromBody] MovieRequest request)
    {
        var created = await service.CreateMovieAsync(request);
        return CreatedAtAction(nameof(GetMovieById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> UpdateMovie(Guid id, [FromBody] MovieRequest request)
    {
        var updated = await service.UpdateMovieAsync(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovie(Guid id)
    {
        var deleted = await service.DeleteMovieAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
