using Microsoft.AspNetCore.Mvc;
using Screening.API.Entities;

namespace Screening.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ScreeningController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Entities.Screening>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScreenings(
        [FromQuery] Guid? movieId,
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? cinemaId)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Entities.Screening), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScreeningById(Guid id)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [ProducesResponseType(typeof(Entities.Screening), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScreening([FromBody] Entities.Screening screening)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Entities.Screening), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateScreening(Guid id, [FromBody] Entities.Screening screening)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScreening(Guid id)
    {
        throw new NotImplementedException();
    }
}
