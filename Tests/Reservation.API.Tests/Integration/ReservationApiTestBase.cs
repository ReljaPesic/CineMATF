using System.Net.Http.Headers;
using System.Net.Http.Json;
using Reservation.API.DTOs.Requests;
using Reservation.API.DTOs.Responses;

namespace Reservation.API.Tests.Integration;

public abstract class ReservationApiTestBase(ReservationApiFactory factory)
{
    protected readonly HttpClient Client = factory.CreateClient();

    protected void AuthenticateAs(Guid userId, string role) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateFor(userId, role));

    protected static CreateReservationRequest NewCreateRequest(Guid ownerId) =>
        new(Guid.NewGuid(), [Guid.NewGuid()], ownerId);

    // Authenticates as the given owner and creates a reservation for themselves -
    // the setup step every ownership test in this suite needs.
    protected async Task<ReservationResponse> SeedReservationAsync(Guid ownerId)
    {
        AuthenticateAs(ownerId, TestJwt.UserRole);
        var response = await Client.PostAsJsonAsync("/api/v1/reservations", NewCreateRequest(ownerId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReservationResponse>())!;
    }
}
