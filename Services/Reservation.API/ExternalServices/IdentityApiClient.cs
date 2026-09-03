using System.Net;
using System.Net.Http.Json;

namespace Reservation.API.ExternalServices;

public class IdentityApiClient(HttpClient httpClient) : IIdentityApiClient
{
    private record UserContactResponse(string Id, string Email, string FirstName, string LastName);

    public async Task<UserContactDetails?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/v1/user/by-id/{userId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserContactResponse>(cancellationToken);
        return user == null ? null : new UserContactDetails(Guid.Parse(user.Id), user.Email, user.FirstName, user.LastName);
    }
}
