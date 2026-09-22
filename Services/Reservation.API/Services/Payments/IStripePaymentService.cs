namespace Reservation.API.Services.Payments;

public interface IStripePaymentService
{
    Task<(string SessionId, string Url)> CreateCheckoutSessionAsync(
        Guid reservationId, decimal totalPrice, int seatCount, DateTime sessionExpiresAt, CancellationToken cancellationToken = default);

    // Verifies the Stripe-Signature header and returns the parts of the event we care
    // about. Throws if the signature is invalid so callers can reject the webhook.
    StripeWebhookEvent ConstructEvent(string json, string signatureHeader);

    Task RefundAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}

public sealed record StripeWebhookEvent(string Type, Guid? ReservationId, string? SessionId, string? PaymentIntentId);
