using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Reservation.API.Authorization;

public static class ClaimsPrincipalExtensions
{
    // Identity.API puts the user's id in the JWT "sub" claim. Depending on JwtBearer's
    // inbound claim mapping, that can surface as ClaimTypes.NameIdentifier or as "sub" -
    // check both.
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin);

    public static bool CanAccessUser(this ClaimsPrincipal user, Guid resourceUserId) =>
        user.IsAdmin() || user.GetUserId() == resourceUserId;
}
