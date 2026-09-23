using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Reservation.API.Authorization;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;
using Reservation.API.Services;

namespace Reservation.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/reservations")]
public class ReservationController(IReservationService service, IHostEnvironment environment) : ControllerBase
{
    // SuperAdmin sees every reservation, CinemaAdmin sees their cinema's, a regular user only their own.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetAllReservations()
    {
        if (User.IsSuperAdmin())
        {
            return Ok(await service.GetAllReservationsAsync());
        }

        if (User.IsCinemaAdmin())
        {
            return Ok(await service.GetReservationsByCinemaIdsAsync(User.GetCinemaIds()));
        }

        var userId = User.GetUserId();
        if (userId is null) return Forbid();

        return Ok(await service.GetReservationsByUserIdAsync(userId.Value));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetReservationById(Guid id)
    {
        var reservation = await service.GetReservationByIdAsync(id);
        if (reservation == null) return NotFound();
        if (!await CanAccessReservationAsync(reservation)) return Forbid();
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

    // A user may only reserve seats for themselves; admins may reserve on behalf of anyone.
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        if (!User.CanAccessUser(request.UserId)) return Forbid();

        var result = await service.CreateReservationAsync(request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetReservationById), new { id = result.Response!.Id }, result.Response);
    }

    // Dev/Testing-only bypass for the real Stripe Checkout flow below (see CreateCheckoutSession).
    // Kept because existing tests use it as setup plumbing to reach a Confirmed reservation.
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(Guid id)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing")) return NotFound();

        var reservation = await service.GetReservationByIdAsync(id);
        if (reservation == null) return NotFound(new { message = "Reservation not found" });
        if (!await CanAccessReservationAsync(reservation)) return Forbid();

        var (Success, ErrorMessage) = await service.PayAsync(id);
        if (!Success)
            return ErrorMessage == "Reservation not found"
                ? NotFound(new { message = ErrorMessage })
                : BadRequest(new { message = ErrorMessage });

        return Ok();
    }

    [HttpPost("{id:guid}/checkout-session")]
    [ProducesResponseType(typeof(CheckoutSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCheckoutSession(Guid id)
    {
        var reservation = await service.GetReservationByIdAsync(id);
        if (reservation == null) return NotFound(new { message = "Reservation not found" });
        if (!User.CanAccessUser(reservation.UserId)) return Forbid();

        var (Success, ErrorMessage, Response) = await service.CreateCheckoutSessionAsync(id);
        if (!Success)
            return ErrorMessage == "Reservation not found"
                ? NotFound(new { message = ErrorMessage })
                : BadRequest(new { message = ErrorMessage });

        return Ok(Response);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        var reservation = await service.GetReservationByIdAsync(id);
        if (reservation == null) return NotFound(new { message = "Reservation not found" });
        if (!await CanAccessReservationAsync(reservation)) return Forbid();

        var (Success, ErrorMessage) = await service.CancelReservationAsync(id);
        if (!Success)
            return ErrorMessage == "Reservation not found"
                ? NotFound(new { message = ErrorMessage })
                : BadRequest(new { message = ErrorMessage });

        return Ok();
    }

    // Resolves cinema-scoped access for a CinemaAdmin on top of the plain self-or-SuperAdmin rule.
    private async Task<bool> CanAccessReservationAsync(ReservationResponse reservation)
    {
        if (User.CanAccessUser(reservation.UserId)) return true;
        if (!User.IsCinemaAdmin()) return false;

        var screeningCinemaId = await service.GetScreeningCinemaIdAsync(reservation.ScreeningId);
        return screeningCinemaId is { } cinemaId && User.GetCinemaIds().Contains(cinemaId);
    }
}
