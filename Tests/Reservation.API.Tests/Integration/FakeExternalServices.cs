using System.Text.Json;
using Reservation.API.ExternalServices;
using Reservation.API.Services.Payments;

namespace Reservation.API.Tests.Integration;

// Reservation.API validates screenings/seats against Cinema.API, Movie.API and
// Screening.API over the network. These fakes stand in for those services so the
// ownership/authorization tests can exercise the real ReservationService/Repository
// against an in-memory database without any other service running.
internal class FakeScreeningApiClient : IScreeningApiClient
{
    public Task<ScreeningDetails?> GetScreeningAsync(Guid screeningId, CancellationToken cancellationToken = default) =>
        Task.FromResult<ScreeningDetails?>(new ScreeningDetails(
            screeningId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), "2D"));
}

internal class FakeCinemaApiClient : ICinemaApiClient
{
    public Task<SeatDetails?> GetSeatAsync(Guid seatId, CancellationToken cancellationToken = default) =>
        Task.FromResult<SeatDetails?>(new SeatDetails(seatId, 1, 1, "Standard"));

    public Task<IEnumerable<SeatDetails>> GetSeatsByHallAsync(Guid cinemaId, Guid hallId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<SeatDetails>>([]);

    public Task<CinemaDetails?> GetCinemaAsync(Guid cinemaId, CancellationToken cancellationToken = default) =>
        Task.FromResult<CinemaDetails?>(new CinemaDetails(cinemaId, "CineMax", "Beograd"));
}

internal class FakeMovieApiClient : IMovieApiClient
{
    public Task<MovieDetails?> GetMovieAsync(Guid movieId, CancellationToken cancellationToken = default) =>
        Task.FromResult<MovieDetails?>(new MovieDetails(movieId, "Test Movie"));
}

internal class FakeIdentityApiClient : IIdentityApiClient
{
    public Task<UserContactDetails?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<UserContactDetails?>(new UserContactDetails(userId, "test-user@example.com", "Test", "User"));
}

// Stands in for the real Stripe SDK. Rather than real Stripe event JSON + HMAC signatures,
// tests post a small JSON shape of their own; ConstructEvent below deserializes it directly.
// A signature header of "invalid" simulates a signature-verification failure.
public class FakeStripePaymentService : IStripePaymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public List<string> RefundedPaymentIntentIds { get; } = [];

    public Task<(string SessionId, string Url)> CreateCheckoutSessionAsync(
        Guid reservationId, decimal totalPrice, int seatCount, DateTime sessionExpiresAt, CancellationToken cancellationToken = default) =>
        Task.FromResult(($"cs_test_{reservationId:N}", $"https://checkout.stripe.com/pay/cs_test_{reservationId:N}"));

    public StripeWebhookEvent ConstructEvent(string json, string signatureHeader)
    {
        if (signatureHeader == "invalid")
        {
            throw new InvalidOperationException("Simulated invalid Stripe webhook signature");
        }

        var payload = JsonSerializer.Deserialize<FakeWebhookPayload>(json, JsonOptions)
            ?? throw new InvalidOperationException("Empty webhook payload");
        return new StripeWebhookEvent(payload.Type, payload.ReservationId, payload.SessionId, payload.PaymentIntentId);
    }

    public Task RefundAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        RefundedPaymentIntentIds.Add(paymentIntentId);
        return Task.CompletedTask;
    }

    internal record FakeWebhookPayload(string Type, Guid? ReservationId, string? SessionId, string? PaymentIntentId);
}
