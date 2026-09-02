using System.Net;
using System.Net.Http.Headers;

namespace Reservation.API.Tests.Integration;

public class TicketOwnershipTests : IClassFixture<ReservationApiFactory>
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private readonly ReservationApiFactory _factory;
    private readonly HttpClient _client;

    public TicketOwnershipTests(ReservationApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void AuthenticateAs(Guid userId, string role = "User") =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwt.CreateFor(userId, role));

    private async Task<(Guid ReservationId, Guid TicketId)> SeedReservationWithTicketAsync(Guid ownerId)
    {
        var reservationId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        await _factory.SeedAsync(db =>
        {
            db.Reservations.Add(new Entities.Reservation
            {
                Id = reservationId,
                UserId = ownerId,
                ScreeningId = Guid.NewGuid(),
                Status = ReservationStatus.Confirmed,
                TotalPrice = 20,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            });
            db.Tickets.Add(new Entities.Ticket
            {
                Id = ticketId,
                ReservationId = reservationId,
                SeatId = Guid.NewGuid(),
                SeatRow = 1,
                SeatNumber = 1,
                Price = 10,
                QrCode = Guid.NewGuid().ToString(),
            });
        });
        return (reservationId, ticketId);
    }

    [Fact]
    public async Task GetTicket_ReturnsOk_WhenCallerIsOwner()
    {
        var (_, ticketId) = await SeedReservationWithTicketAsync(OwnerId);
        AuthenticateAs(OwnerId);

        var response = await _client.GetAsync($"/api/v1/Ticket/{ticketId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTicket_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var (_, ticketId) = await SeedReservationWithTicketAsync(OwnerId);
        AuthenticateAs(OtherUserId);

        var response = await _client.GetAsync($"/api/v1/Ticket/{ticketId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTicket_ReturnsUnauthorized_WhenNoToken()
    {
        var (_, ticketId) = await SeedReservationWithTicketAsync(OwnerId);

        var response = await _client.GetAsync($"/api/v1/Ticket/{ticketId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTicketsByReservation_ReturnsOk_WhenCallerIsOwner()
    {
        var (reservationId, _) = await SeedReservationWithTicketAsync(OwnerId);
        AuthenticateAs(OwnerId);

        var response = await _client.GetAsync($"/api/v1/Ticket/reservation/{reservationId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTicketsByReservation_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var (reservationId, _) = await SeedReservationWithTicketAsync(OwnerId);
        AuthenticateAs(OtherUserId);

        var response = await _client.GetAsync($"/api/v1/Ticket/reservation/{reservationId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTicketsByReservation_ReturnsNotFound_WhenReservationDoesNotExist()
    {
        AuthenticateAs(OwnerId);

        var response = await _client.GetAsync($"/api/v1/Ticket/reservation/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DownloadTicket_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var (_, ticketId) = await SeedReservationWithTicketAsync(OwnerId);
        AuthenticateAs(OtherUserId);

        var response = await _client.GetAsync($"/api/v1/Ticket/{ticketId}/download");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTicketsForReservation_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var (reservationId, _) = await SeedReservationWithTicketAsync(OwnerId);
        AuthenticateAs(OtherUserId);

        var response = await _client.PostAsync($"/api/v1/Ticket/reservation/{reservationId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllTickets_ReturnsForbidden_WhenCallerIsNotAdmin()
    {
        AuthenticateAs(OwnerId);

        var response = await _client.GetAsync("/api/v1/Ticket");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllTickets_ReturnsOk_WhenCallerIsAdmin()
    {
        AuthenticateAs(OwnerId, "Admin");

        var response = await _client.GetAsync("/api/v1/Ticket");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
