using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Reservation.API.Tests.Integration;

public class ReservationOwnershipTests : IClassFixture<ReservationApiFactory>
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private readonly ReservationApiFactory _factory;
    private readonly HttpClient _client;

    public ReservationOwnershipTests(ReservationApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void AuthenticateAs(Guid userId, string role = "User") =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.CreateFor(userId, role));

    private async Task<Guid> SeedReservationAsync(
        Guid ownerId,
        ReservationStatus status = ReservationStatus.Locked,
        DateTime? expiresAt = null)
    {
        var reservationId = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Reservations.Add(new Entities.Reservation
            {
                Id = reservationId,
                UserId = ownerId,
                ScreeningId = Guid.NewGuid(),
                Status = status,
                TotalPrice = 20,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(10),
            });
        });
        return reservationId;
    }

    [Fact]
    public async Task GetReservationById_ReturnsUnauthorized_WhenNoToken()
    {
        var id = await SeedReservationAsync(OwnerId);

        var response = await _client.GetAsync($"/api/v1/reservations/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenCallerIsOwner()
    {
        var id = await SeedReservationAsync(OwnerId);
        AuthenticateAs(OwnerId);

        var response = await _client.GetAsync($"/api/v1/reservations/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReservationById_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var id = await SeedReservationAsync(OwnerId);
        AuthenticateAs(OtherUserId);

        var response = await _client.GetAsync($"/api/v1/reservations/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenCallerIsAdmin()
    {
        var id = await SeedReservationAsync(OwnerId);
        AuthenticateAs(OtherUserId, "Admin");

        var response = await _client.GetAsync($"/api/v1/reservations/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pay_ReturnsOk_WhenCallerIsOwner()
    {
        var id = await SeedReservationAsync(OwnerId, ReservationStatus.Locked, DateTime.UtcNow.AddMinutes(10));
        AuthenticateAs(OwnerId);

        var response = await _client.PostAsync($"/api/v1/reservations/{id}/pay", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pay_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var id = await SeedReservationAsync(OwnerId, ReservationStatus.Locked, DateTime.UtcNow.AddMinutes(10));
        AuthenticateAs(OtherUserId);

        var response = await _client.PostAsync($"/api/v1/reservations/{id}/pay", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cancel_ReturnsOk_WhenCallerIsOwner()
    {
        var id = await SeedReservationAsync(OwnerId, ReservationStatus.Locked, DateTime.UtcNow.AddMinutes(10));
        AuthenticateAs(OwnerId);

        var response = await _client.PostAsync($"/api/v1/reservations/{id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cancel_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var id = await SeedReservationAsync(OwnerId, ReservationStatus.Locked, DateTime.UtcNow.AddMinutes(10));
        AuthenticateAs(OtherUserId);

        var response = await _client.PostAsync($"/api/v1/reservations/{id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateReservation_ReturnsUnauthorized_WhenNoToken()
    {
        var request = new { screeningId = Guid.NewGuid(), seatIds = new[] { Guid.NewGuid() }, userId = OwnerId };

        var response = await _client.PostAsJsonAsync("/api/v1/reservations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReservation_ReturnsForbidden_WhenUserIdInBodyIsNotCallersOwnId()
    {
        AuthenticateAs(OwnerId);
        var request = new { screeningId = Guid.NewGuid(), seatIds = new[] { Guid.NewGuid() }, userId = OtherUserId };

        var response = await _client.PostAsJsonAsync("/api/v1/reservations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllReservations_ReturnsOnlyOwnReservations_ForNonAdmin()
    {
        var ownReservationId = await SeedReservationAsync(OwnerId);
        await SeedReservationAsync(OtherUserId);
        AuthenticateAs(OwnerId);

        var response = await _client.GetAsync("/api/v1/reservations");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservations = await response.Content.ReadFromJsonAsync<List<ReservationResponse>>();
        reservations.Should().NotBeNull();
        reservations!.Should().OnlyContain(r => r.UserId == OwnerId);
        reservations.Should().Contain(r => r.Id == ownReservationId);
    }

    [Fact]
    public async Task GetAllReservations_ReturnsAllReservations_ForAdmin()
    {
        var reservation1 = await SeedReservationAsync(OwnerId);
        var reservation2 = await SeedReservationAsync(OtherUserId);
        AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await _client.GetAsync("/api/v1/reservations");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservations = await response.Content.ReadFromJsonAsync<List<ReservationResponse>>();
        reservations.Should().NotBeNull();
        reservations!.Select(r => r.Id).Should().Contain([reservation1, reservation2]);
    }
}
