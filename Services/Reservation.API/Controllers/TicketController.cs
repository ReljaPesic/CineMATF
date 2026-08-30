using Microsoft.AspNetCore.Mvc;
using Reservation.API.DTOs.Responses;
using Reservation.API.Services;

namespace Reservation.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TicketController(IReservationService service) : ControllerBase
{
    private readonly IReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAllTickets()
    {
        var tickets = await _service.GetAllTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetTicket(Guid id)
    {
        var ticket = await _service.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        return Ok(ticket);
    }

    [HttpGet("reservation/{reservationId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetTicketsByReservation(Guid reservationId)
    {
        var tickets = await _service.GetReservationTicketsAsync(reservationId);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadTicket(Guid id)
    {
        var (success, errorMessage, content, fileName) = await _service.GetTicketFileAsync(id);
        if (!success)
            return errorMessage == "Ticket not found" || errorMessage == "Screening not found"
                ? NotFound(new { message = errorMessage })
                : BadRequest(new { message = errorMessage });

        return File(content!, "text/plain", fileName);
    }

}
