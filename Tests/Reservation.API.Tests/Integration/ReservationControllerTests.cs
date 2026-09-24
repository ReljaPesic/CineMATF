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
        AuthenticateAs(Guid.NewGuid(), TestJwt.SuperAdminRole);

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
        AuthenticateAs(Guid.NewGuid(), TestJwt.SuperAdminRole);

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

        AuthenticateAs(Guid.NewGuid(), TestJwt.SuperAdminRole);
        var response = await Client.GetAsync("/api/v1/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservations = await response.Content.ReadFromJsonAsync<List<ReservationResponse>>();
        reservations!.Select(r => r.Id).Should().Contain(reservation.Id);
    }

    [Fact]
    public async Task GetAllReservations_OnlyReturnsOwnCinemasReservations_ForCinemaAdmin()
    {
        var ownCinemaId = Guid.NewGuid();
        var otherCinemaId = Guid.NewGuid();
        var ownScreeningId = Guid.NewGuid();
        var otherScreeningId = Guid.NewGuid();
        SetScreeningCinema(ownScreeningId, ownCinemaId);
        SetScreeningCinema(otherScreeningId, otherCinemaId);

        var ownCinemaReservation = await SeedReservationAsync(Guid.NewGuid(), ownScreeningId);
        var otherCinemaReservation = await SeedReservationAsync(Guid.NewGuid(), otherScreeningId);

        AuthenticateAs(Guid.NewGuid(), TestJwt.CinemaAdminRole, ownCinemaId);
        var response = await Client.GetAsync("/api/v1/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservations = (await response.Content.ReadFromJsonAsync<List<ReservationResponse>>())!;
        reservations.Select(r => r.Id).Should().Contain(ownCinemaReservation.Id);
        reservations.Select(r => r.Id).Should().NotContain(otherCinemaReservation.Id);
    }

    // One admin can manage more than one cinema - reservations from either of their cinemas
    // should show up, while a third cinema they don't manage stays hidden.
    [Fact]
    public async Task GetAllReservations_ReturnsReservationsFromBothManagedCinemas_ForCinemaAdminWithTwoCinemas()
    {
        var firstCinemaId = Guid.NewGuid();
        var secondCinemaId = Guid.NewGuid();
        var unmanagedCinemaId = Guid.NewGuid();
        var firstScreeningId = Guid.NewGuid();
        var secondScreeningId = Guid.NewGuid();
        var unmanagedScreeningId = Guid.NewGuid();
        SetScreeningCinema(firstScreeningId, firstCinemaId);
        SetScreeningCinema(secondScreeningId, secondCinemaId);
        SetScreeningCinema(unmanagedScreeningId, unmanagedCinemaId);

        var firstReservation = await SeedReservationAsync(Guid.NewGuid(), firstScreeningId);
        var secondReservation = await SeedReservationAsync(Guid.NewGuid(), secondScreeningId);
        var unmanagedReservation = await SeedReservationAsync(Guid.NewGuid(), unmanagedScreeningId);

        AuthenticateAs(Guid.NewGuid(), TestJwt.CinemaAdminRole, [firstCinemaId, secondCinemaId]);
        var response = await Client.GetAsync("/api/v1/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservations = (await response.Content.ReadFromJsonAsync<List<ReservationResponse>>())!;
        reservations.Select(r => r.Id).Should().Contain(firstReservation.Id);
        reservations.Select(r => r.Id).Should().Contain(secondReservation.Id);
        reservations.Select(r => r.Id).Should().NotContain(unmanagedReservation.Id);
    }

    [Fact]
    public async Task GetReservationById_ReturnsOk_WhenCallerIsCinemaAdminOfItsScreeningsCinema()
    {
        var cinemaId = Guid.NewGuid();
        var screeningId = Guid.NewGuid();
        SetScreeningCinema(screeningId, cinemaId);
        var reservation = await SeedReservationAsync(Guid.NewGuid(), screeningId);

        AuthenticateAs(Guid.NewGuid(), TestJwt.CinemaAdminRole, cinemaId);
        var response = await Client.GetAsync($"/api/v1/reservations/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReservationById_ReturnsForbidden_WhenCallerIsCinemaAdminOfADifferentCinema()
    {
        var screeningId = Guid.NewGuid();
        SetScreeningCinema(screeningId, Guid.NewGuid());
        var reservation = await SeedReservationAsync(Guid.NewGuid(), screeningId);

        AuthenticateAs(Guid.NewGuid(), TestJwt.CinemaAdminRole, Guid.NewGuid());
        var response = await Client.GetAsync($"/api/v1/reservations/{reservation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
