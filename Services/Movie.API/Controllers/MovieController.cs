using Microsoft.AspNetCore.Mvc;
using Movie.API.Entities;
using Movie.API.Repositories;

namespace Movie.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MovieController(IMovieRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Entities.Movie>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Entities.Movie>>> GetMovies()
    {
        var movies = await repository.GetMoviesAsync();
        return Ok(movies);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Entities.Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entities.Movie>> GetMovieById(Guid id)
    {
        var movie = await repository.GetMovieByIdAsync(id);
        if (movie == null) return NotFound();
        return Ok(movie);
    }

    [HttpGet("genre/{genre}")]
    [ProducesResponseType(typeof(IEnumerable<Entities.Movie>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Entities.Movie>>> GetMoviesByGenre(string genre)
    {
        if (!Enum.TryParse<Genre>(genre, ignoreCase: true, out var parsedGenre))
            return BadRequest(new { message = "Invalid genre." });

        var movies = await repository.GetMoviesByGenreAsync(parsedGenre);
        return Ok(movies);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Entities.Movie>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Entities.Movie>>> GetMoviesByTitle([FromQuery] string title)
    {
        var movies = await repository.GetMoviesByTitleAsync(title);
        return Ok(movies);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Entities.Movie), StatusCodes.Status201Created)]
    public async Task<ActionResult<Entities.Movie>> CreateMovie([FromBody] Entities.Movie movie)
    {
        var created = await repository.CreateMovieAsync(movie);
        return CreatedAtAction(nameof(GetMovieById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Entities.Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entities.Movie>> UpdateMovie(Guid id, [FromBody] Entities.Movie movie)
    {
        movie.Id = id;
        var updated = await repository.UpdateMovieAsync(movie);
        if (!updated) return NotFound();
        return Ok(movie);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMovie(Guid id)
    {
        var deleted = await repository.DeleteMovieAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
