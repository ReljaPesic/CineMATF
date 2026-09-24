using System.Security.Claims;

namespace Cinema.API.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.SuperAdmin);

    public static bool IsCinemaAdmin(this ClaimsPrincipal user) => user.IsInRole(Roles.CinemaAdmin);

    // Identity.API puts one "cinemaId" claim per cinema a CinemaAdmin manages.
    public static IReadOnlySet<Guid> GetCinemaIds(this ClaimsPrincipal user) =>
        user.FindAll("cinemaId")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

    // SuperAdmin can manage every cinema; a CinemaAdmin only the ones they're scoped to.
    public static bool CanManageCinema(this ClaimsPrincipal user, Guid cinemaId) =>
        user.IsSuperAdmin() || user.GetCinemaIds().Contains(cinemaId);
}
