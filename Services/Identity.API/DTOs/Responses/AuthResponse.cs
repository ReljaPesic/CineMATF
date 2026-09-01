namespace Identity.API.DTOs.Responses;

// What POST /api/v1/Auth/Login, /Refresh return on success.
// AccessToken  - short-lived JWT, sent as "Authorization: Bearer ..." afterwards.
// RefreshToken - long-lived opaque string, exchanged for a new pair via /Refresh.
public class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}