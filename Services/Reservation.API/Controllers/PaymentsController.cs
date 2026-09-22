using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.API.Services;

namespace Reservation.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[AllowAnonymous]
public class PaymentsController(IReservationService service, ILogger<PaymentsController> logger) : ControllerBase
{
    // Called directly by Stripe - no JWT, and signature verification needs the exact raw
    // body, so this deliberately takes no bound request model.
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var (success, errorMessage) = await service.ConfirmPaymentFromWebhookAsync(json, signature);
        if (!success)
        {
            logger.LogWarning("Stripe webhook rejected: {Error}", errorMessage);
            return BadRequest();
        }

        // Always 200 once handled - Stripe retries on any non-2xx response.
        return Ok();
    }
}
