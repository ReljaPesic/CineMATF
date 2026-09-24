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

    public const string SuperAdminRole = "SuperAdmin";
    public const string CinemaAdminRole = "CinemaAdmin";
    public const string UserRole = "User";

    public static string CreateFor(Guid userId, string role = UserRole, Guid? cinemaId = null) =>
        CreateFor(userId, role, cinemaId.HasValue ? [cinemaId.Value] : []);

    public static string CreateFor(Guid userId, string role, IEnumerable<Guid> cinemaIds)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Name, $"test-{role.ToLowerInvariant()}-{userId:N}"),
            new(ClaimTypes.Role, role),
        };

        claims.AddRange(cinemaIds.Select(id => new Claim("cinemaId", id.ToString())));

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
