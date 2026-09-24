using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;
using Reservation.API.ExternalServices;

namespace Reservation.API.Tests.Integration;

public abstract class ReservationApiTestBase(ReservationApiFactory factory)
{
    protected readonly HttpClient Client = factory.CreateClient();

    protected void AuthenticateAs(Guid userId, string role, Guid? cinemaId = null) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateFor(userId, role, cinemaId));

    protected void AuthenticateAs(Guid userId, string role, IEnumerable<Guid> cinemaIds) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateFor(userId, role, cinemaIds));

    protected static CreateReservationRequest NewCreateRequest(Guid ownerId, Guid? screeningId = null) =>
        new(screeningId ?? Guid.NewGuid(), [Guid.NewGuid()], ownerId);

    // Pins the screening's cinema so a CinemaAdmin scoping check has something deterministic to match.
    protected void SetScreeningCinema(Guid screeningId, Guid cinemaId) =>
        ((FakeScreeningApiClient)factory.Services.GetRequiredService<IScreeningApiClient>()).SetCinemaId(screeningId, cinemaId);

    // Authenticates as the given owner and creates a reservation for themselves -
    // the setup step every ownership test in this suite needs.
    protected async Task<ReservationResponse> SeedReservationAsync(Guid ownerId, Guid? screeningId = null)
    {
        AuthenticateAs(ownerId, TestJwt.UserRole);
        var response = await Client.PostAsJsonAsync("/api/v1/reservations", NewCreateRequest(ownerId, screeningId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReservationResponse>())!;
    }
}
