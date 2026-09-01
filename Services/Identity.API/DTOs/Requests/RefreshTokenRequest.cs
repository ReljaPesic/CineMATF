namespace Identity.API.DTOs.Requests;

public class RefreshTokenRequest
{
    public string UserName { get; set; }
    public string RefreshToken { get; set; }
}