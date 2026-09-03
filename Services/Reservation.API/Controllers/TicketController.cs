using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.API.Authorization;
using Reservation.API.DTOs.Responses;
using Reservation.API.Services;

namespace Reservation.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TicketController(IReservationService service) : ControllerBase
{
    private readonly IReservationService _service = service ?? throw new ArgumentNullException(nameof(service));

    // Admin-only: every ticket across every user's reservations.
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetAllTickets()
    {
        var tickets = await _service.GetAllTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetTicket(Guid id)
    {
        var ticket = await _service.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        if (!await CanAccessReservationAsync(ticket.ReservationId)) return Forbid();
        return Ok(ticket);
    }

    [HttpGet("reservation/{reservationId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> GetTicketsByReservation(Guid reservationId)
    {
        var reservation = await _service.GetReservationByIdAsync(reservationId);
        if (reservation == null) return NotFound();
        if (!User.CanAccessUser(reservation.UserId)) return Forbid();

        var tickets = await _service.GetReservationTicketsAsync(reservationId);
        return Ok(tickets);
    }

    [HttpPost("reservation/{reservationId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TicketResponse>>> CreateTicketsForReservation(Guid reservationId)
    {
        var reservation = await _service.GetReservationByIdAsync(reservationId);
        if (reservation == null) return NotFound(new { message = "Reservation not found" });
        if (!User.CanAccessUser(reservation.UserId)) return Forbid();

        var (success, errorMessage, tickets) = await _service.GenerateTicketsAsync(reservationId);
        if (!success)
            return errorMessage == "Reservation not found"
                ? NotFound(new { message = errorMessage })
                : BadRequest(new { message = errorMessage });

        return Ok(tickets);
    }

    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadTicket(Guid id)
    {
        var ticket = await _service.GetTicketByIdAsync(id);
        if (ticket == null) return NotFound(new { message = "Ticket not found" });
        if (!await CanAccessReservationAsync(ticket.ReservationId)) return Forbid();

        var (success, errorMessage, content, fileName) = await _service.GetTicketFileAsync(id);
        if (!success)
            return errorMessage == "Ticket not found" || errorMessage == "Screening not found"
                ? NotFound(new { message = errorMessage })
                : BadRequest(new { message = errorMessage });

        return File(content!, "application/pdf", fileName);
    }

    // Resolves who owns a reservation and checks it against the caller (admin or the owner themselves).
    private async Task<bool> CanAccessReservationAsync(Guid reservationId)
    {
        var reservation = await _service.GetReservationByIdAsync(reservationId);
        return reservation != null && User.CanAccessUser(reservation.UserId);
    }

}
