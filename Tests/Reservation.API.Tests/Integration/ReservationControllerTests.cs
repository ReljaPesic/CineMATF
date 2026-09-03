using System.Net;
using System.Net.Http.Json;
using Reservation.API.DTOs.Responses;

namespace Reservation.API.Tests.Integration;

public class ReservationControllerTests(ReservationApiFactory factory)
    : ReservationApiTestBase(factory), IClassFixture<ReservationApiFactory>
{
    [Fact]
    public async Task CreateReservation_ReturnsUnauthorized_WhenNoToken()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/reservations", NewCreateRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReservation_ReturnsForbidden_WhenUserIdDoesNotMatchCaller()
    {
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", NewCreateRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateReservation_Succeeds_WhenUserReservesForSelf()
    {
        var userId = Guid.NewGuid();
        AuthenticateAs(userId, TestJwt.UserRole);

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", NewCreateRequest(userId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ReservationResponse>();
        body!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateReservation_Succeeds_WhenAdminReservesForAnotherUser()
    {
        var otherUser = Guid.NewGuid();
        AuthenticateAs(Guid.NewGuid(), TestJwt.AdminRole);

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", NewCreateRequest(otherUser));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ReservationResponse>();
        body!.UserId.Should().Be(otherUser);
    }

    [Fact]
    public async Task GetReservationById_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.GetAsync($"/api/v1/reservations/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenCallerIsOwner()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);

        var response = await Client.GetAsync($"/api/v1/reservations/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenCallerIsAdmin()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());
        AuthenticateAs(Guid.NewGuid(), TestJwt.AdminRole);

        var response = await Client.GetAsync($"/api/v1/reservations/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllReservations_OnlyReturnsCallersOwnReservations()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var reservationA = await SeedReservationAsync(userA);
        var reservationB = await SeedReservationAsync(userB);

        AuthenticateAs(userA, TestJwt.UserRole);
        var response = await Client.GetAsync("/api/v1/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservations = (await response.Content.ReadFromJsonAsync<List<ReservationResponse>>())!;
        reservations.Select(r => r.Id).Should().Contain(reservationA.Id);
        reservations.Select(r => r.Id).Should().NotContain(reservationB.Id);
    }

    [Fact]
    public async Task GetAllReservations_ReturnsEveryonesReservations_ForAdmin()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());

        AuthenticateAs(Guid.NewGuid(), TestJwt.AdminRole);
        var response = await Client.GetAsync("/api/v1/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservations = await response.Content.ReadFromJsonAsync<List<ReservationResponse>>();
        reservations!.Select(r => r.Id).Should().Contain(reservation.Id);
    }

    [Fact]
    public async Task Pay_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/pay", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Pay_Succeeds_WhenCallerIsOwner()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/pay", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelReservation_ReturnsForbidden_WhenCallerIsNotOwner()
    {
        var reservation = await SeedReservationAsync(Guid.NewGuid());
        AuthenticateAs(Guid.NewGuid(), TestJwt.UserRole);

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CancelReservation_Succeeds_WhenCallerIsOwner()
    {
        var owner = Guid.NewGuid();
        var reservation = await SeedReservationAsync(owner);
        AuthenticateAs(owner, TestJwt.UserRole);

        var response = await Client.PostAsync($"/api/v1/reservations/{reservation.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
