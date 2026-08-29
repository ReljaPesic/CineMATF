using Microsoft.AspNetCore.Mvc;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;
using Reservation.API.Services;

namespace Reservation.API.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public class ReservationController(IReservationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetAllReservations()
    {
        var reservations = await service.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetReservationById(Guid id)
    {
        var reservation = await service.GetReservationByIdAsync(id);
        if (reservation == null) return NotFound();
        return Ok(reservation);
    }

    [HttpGet("screenings/{screeningId:guid}/available-seats")]
    [ProducesResponseType(typeof(AvailableSeatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailableSeatsResponse>> GetAvailableSeats(Guid screeningId)
    {
        var response = await service.GetAvailableSeatsAsync(screeningId);
        if (response == null) return NotFound();
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var result = await service.CreateReservationAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetReservationById), new { id = result.Response!.Id }, result.Response);
    }

    //temporary this is the payment service
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(Guid id)
    {
        var (Success, ErrorMessage) = await service.PayAsync(id);
        if (!Success)
            return ErrorMessage == "Reservation not found"
                ? NotFound(new { message = ErrorMessage })
                : BadRequest(new { message = ErrorMessage });

        return Ok();
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmReservation(Guid id, [FromBody] ConfirmReservationRequest request)
    {
        var (Success, ErrorMessage) = await service.ConfirmReservationAsync(id, request.PaymentId);
        if (!Success)
            return ErrorMessage == "Reservation not found"
                ? NotFound(new { message = ErrorMessage })
                : BadRequest(new { message = ErrorMessage });

        return Ok();
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        var (Success, ErrorMessage) = await service.CancelReservationAsync(id);
        if (!Success)
            return ErrorMessage == "Reservation not found"
                ? NotFound(new { message = ErrorMessage })
                : BadRequest(new { message = ErrorMessage });

        return Ok();
    }

}
