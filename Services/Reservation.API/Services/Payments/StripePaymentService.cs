using Microsoft.Extensions.Options;
using Reservation.API.Settings;
using Stripe;
using Stripe.Checkout;

namespace Reservation.API.Services.Payments;

public class StripePaymentService(StripeClient client, IOptions<StripeSettings> options) : IStripePaymentService
{
    private readonly StripeClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly StripeSettings _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task<(string SessionId, string Url)> CreateCheckoutSessionAsync(
        Guid reservationId, decimal totalPrice, int seatCount, DateTime sessionExpiresAt, CancellationToken cancellationToken = default)
    {
        var createOptions = new SessionCreateOptions
        {
            Mode = "payment",
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = _settings.Currency,
                        UnitAmount = (long)Math.Round(totalPrice * 100, MidpointRounding.AwayFromZero),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"CineMATF reservation ({seatCount} seat{(seatCount == 1 ? "" : "s")})"
                        }
                    }
                }
            ],
            Metadata = new Dictionary<string, string> { ["reservationId"] = reservationId.ToString() },
            SuccessUrl = $"{_settings.SuccessUrlBase}?status=success&reservationId={reservationId}&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{_settings.CancelUrlBase}?status=cancelled&reservationId={reservationId}",
            ExpiresAt = sessionExpiresAt
        };

        var service = new SessionService(_client);
        var session = await service.CreateAsync(createOptions, cancellationToken: cancellationToken);
        return (session.Id, session.Url);
    }

    public StripeWebhookEvent ConstructEvent(string json, string signatureHeader)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json, signatureHeader, _settings.WebhookSecret, throwOnApiVersionMismatch: false);

        Guid? reservationId = null;
        string? sessionId = null;
        string? paymentIntentId = null;

        if (stripeEvent.Data.Object is Session session)
        {
            sessionId = session.Id;
            paymentIntentId = session.PaymentIntentId;
            if (session.Metadata != null
                && session.Metadata.TryGetValue("reservationId", out var raw)
                && Guid.TryParse(raw, out var parsed))
            {
                reservationId = parsed;
            }
        }

        return new StripeWebhookEvent(stripeEvent.Type, reservationId, sessionId, paymentIntentId);
    }

    public async Task RefundAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var service = new RefundService(_client);
        await service.CreateAsync(new RefundCreateOptions { PaymentIntent = paymentIntentId }, cancellationToken: cancellationToken);
    }
}
