using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Reservation.API.Tests.Integration;

// Mirrors Identity.API's TokenService so tests can mint tokens Reservation.API's
// JwtBearer handler (configured with the same Issuer/Audience/SecretKey in
// appsettings.json) will accept.
internal static class TestJwt
{
    private const string Issuer = "CineMATF.Identity";
    private const string Audience = "CineMATF.Services";
    private const string SecretKey = "MyVerySecretMessageThatOnlyIKnow";

    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static string CreateFor(Guid userId, string role = UserRole)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}-{userId:N}"),
            new(ClaimTypes.Role, role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
