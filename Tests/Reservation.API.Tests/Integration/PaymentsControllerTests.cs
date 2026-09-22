using System.Net;
using System.Net.Http.Json;
using Reservation.API.DTOs.Responses;

namespace Reservation.API.Tests.Integration;

public class PaymentsControllerTests(ReservationApiFactory factory)
    : ReservationApiTestBase(factory), IClassFixture<ReservationApiFactory>
{
    private readonly ReservationApiFactory _factory = factory;

    private static object WebhookPayload(string type, Guid? reservationId, string? sessionId = "cs_test_123", string? paymentIntentId = "pi_test_123") =>
        new { type, reservationId, sessionId, paymentIntentId };

    private Task<HttpResponseMessage> PostWebhookAsync(object payload, string signature = "valid")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments/webhook")
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Add("Stripe-Signature", signature);
        return Client.SendAsync(request);
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/checkout-session", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCheckoutSession_Succeeds_WhenCallerIsOwner()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/checkout-session", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CheckoutSessionResponse>();
        body!.SessionId.Should().NotBeNullOrEmpty();
        body.Url.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateCheckoutSession_ReturnsBadRequest_WhenReservationAlreadyConfirmed()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);
        (await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/pay", null)).EnsureSuccessStatusCode();

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/checkout-session", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Webhook_ReturnsBadRequest_WhenSignatureIsInvalid()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());

        var response = await PostWebhookAsync(
            WebhookPayload("checkout.session.completed", reservation.Id), signature: "invalid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Webhook_ConfirmsLockedReservation()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);

        var response = await PostWebhookAsync(WebhookPayload("checkout.session.completed", reservation.Id));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AuthenticateAs(owner, TestJwt.UserRole);
        var getResponse = await Client.GetFromJsonAsync<ReservationResponse>($"/api/v1/reservations/{reservation.Id}");
        getResponse!.Status.Should().Be(nameof(ReservationStatus.Confirmed));
    }

    [Fact]
    public async Task Webhook_IsIdempotent_WhenTheSameEventIsRedelivered()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        var payload = WebhookPayload("checkout.session.completed", reservation.Id);

        (await PostWebhookAsync(payload)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await PostWebhookAsync(payload)).StatusCode.Should().Be(HttpStatusCode.OK);

        AuthenticateAs(owner, TestJwt.UserRole);
        var getResponse = await Client.GetFromJsonAsync<ReservationResponse>($"/api/v1/reservations/{reservation.Id}");
        getResponse!.Status.Should().Be(nameof(ReservationStatus.Confirmed));
    }

    [Fact]
    public async Task Webhook_RefundsAndDoesNotConfirm_WhenReservationAlreadyCancelled()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);
        (await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/cancel", null)).EnsureSuccessStatusCode();

        var response = await PostWebhookAsync(WebhookPayload("checkout.session.completed", reservation.Id, paymentIntentId: "pi_refund_me"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetFromJsonAsync<ReservationResponse>($"/api/v1/reservations/{reservation.Id}");
        getResponse!.Status.Should().Be(nameof(ReservationStatus.Cancelled));
        _factory.StripePaymentService.RefundedPaymentIntentIds.Should().Contain("pi_refund_me");
    }

    [Fact]
    public async Task Webhook_IgnoresUnrelatedEventTypes()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());

        var response = await PostWebhookAsync(WebhookPayload("payment_intent.created", reservation.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
