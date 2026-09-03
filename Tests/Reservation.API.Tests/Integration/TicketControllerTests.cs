using System.Net;
using System.Net.Http.Json;

namespace Reservation.API.Tests.Integration;

public class TicketControllerTests(ReservationApiFactory factory)
    : ReservationApiTestBase(factory), IClassFixture<ReservationApiFactory>
{
    // Pays for the seeded reservation and generates its tickets so ticket endpoints
    // have something to look up. Leaves the client authenticated as the owner.
    private async Task<ReservationResponse> SeedConfirmedReservationWithTicketsAsync(Guid ownerId)
    {
        var reservation = await SeedReservationAsync(ownerId);

        AuthenticateAs(ownerId, TestJwt.UserRole);
        (await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/pay", null)).EnsureSuccessStatusCode();
        var ticketsResponse = await Client.PostAsync($"/api/v1/Ticket/reservation/{reservation.Id}", null);
        ticketsResponse.EnsureSuccessStatusCode();

        return reservation;
    }

    [Fact]
    public async Task GetTicketsByReservation_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var reservation = await SeedConfirmedReservationWithTicketsAsync(Guid.NewGuid());
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.GetAsync($"/api/v1/Ticket/reservation/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTicketsByReservation_ReturnsOk_WhenCallerIsOwner()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedConfirmedReservationWithTicketsAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);

        var response = await Client.GetAsync($"/api/v1/Ticket/reservation/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTicket_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var reservation = await SeedConfirmedReservationWithTicketsAsync(Guid.NewGuid());
        var ticket = (await (await Client.GetAsync($"/api/v1/Ticket/reservation/{reservation.Id}"))
            .Content.ReadFromJsonAsync<List<TicketResponse>>())!.Single();

        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);
        var response = await Client.GetAsync($"/api/v1/Ticket/{ticket.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTicket_ReturnsOk_WhenCallerIsOwner()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedConfirmedReservationWithTicketsAsync(owner);
        var ticket = (await (await Client.GetAsync($"/api/v1/Ticket/reservation/{reservation.Id}"))
            .Content.ReadFromJsonAsync<List<TicketResponse>>())!.Single();

        AuthenticateAs(owner, TestJwt.UserRole);
        var response = await Client.GetAsync($"/api/v1/Ticket/{ticket.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllTickets_ReturnsForbidden_ForNonAdmin()
    {
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.GetAsync("/api/v1/Ticket");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllTickets_ReturnsOk_ForAdmin()
    {
        AuthenticateAs(Guid.NewGuid(), TestJwt.AdminRole);

        var response = await Client.GetAsync("/api/v1/Ticket");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
