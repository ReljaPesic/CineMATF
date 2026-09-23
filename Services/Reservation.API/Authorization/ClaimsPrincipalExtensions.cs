using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Reservation.API.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.SuperAdmin);

    public static bool IsCinemaAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.CinemaAdmin);

    // Identity.API puts one "cinemaId" claim per cinema a CinemaAdmin manages.
    public static IReadOnlySet<Guid> GetCinemaIds(this ClaimsPrincipal user) =>
        user.FindAll("cinemaId")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

    public static bool CanAccessUser(this ClaimsPrincipal user, Guid resourceUserId) =>
        user.IsSuperAdmin() || user.GetUserId() == resourceUserId;
}
