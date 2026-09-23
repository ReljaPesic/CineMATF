using Identity.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data;

// Seeds the roles and a couple of ready-to-use accounts on first run.
// Roles live here (not EF HasData) so their Ids can be DB-generated without
// making the model non-deterministic - see the note that used to be in
// RoleConfigurations.
public static class IdentityDataSeeder
{
    private const string DefaultPassword = "Passw0rd!";

    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        await EnsureRoleAsync(roleManager, Roles.CinemaAdmin);
        await EnsureRoleAsync(roleManager, Roles.SuperAdmin);
        await EnsureRoleAsync(roleManager, Roles.User);

        // Owner of reservation 33333333-...-333333333333 (Confirmed) in Reservation.API
        await EnsureUserAsync(
            userManager,
            id: "dddddddd-dddd-dddd-dddd-dddddddddddd",
            userName: "alice",
            email: "alice@cinematf.local",
            firstName: "Alice",
            lastName: "Anderson",
            cardNumber: "4111111111111111",
            role: Roles.User);

        // Owner of reservation 33333333-...-333333333334 (Locked) in Reservation.API
        await EnsureUserAsync(
            userManager,
            id: "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
            userName: "bob",
            email: "bob@cinematf.local",
            firstName: "Bob",
            lastName: "Brown",
            cardNumber: "5500000000000004",
            role: Roles.User);

        // SuperAdmin account for exercising the SuperAdmin-only endpoints
        await EnsureUserAsync(
            userManager,
            id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            userName: "admin",
            email: "admin@cinematf.local",
            firstName: "Admin",
            lastName: "CineMATF",
            cardNumber: "340000000000009",
            role: Roles.SuperAdmin);

        await EnsureUserAsync(
            userManager,
            id: "ffffffff-ffff-ffff-ffff-ffffffffffff",
            userName: "cinemaadmin",
            email: "cinemaadmin@cinematf.local",
            firstName: "Cinema",
            lastName: "Admin",
            cardNumber: "370000000000002",
            role: Roles.CinemaAdmin,
            cinemaIds:
            [
                Guid.Parse("cccccccc-cccc-cccc-cccc-000000000001"),
                Guid.Parse("cccccccc-cccc-cccc-cccc-000000000002")
            ]);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string name)
    {
        if (!await roleManager.RoleExistsAsync(name))
        {
            await roleManager.CreateAsync(new IdentityRole(name));
        }
    }

    private static async Task EnsureUserAsync(UserManager<User> userManager, string id, string userName, string email, string firstName, string lastName, string cardNumber, string role, List<Guid>? cinemaIds = null)
    {
        if (await userManager.FindByIdAsync(id) is not null)
        {
            return;
        }

        var user = new User
        {
            Id = id,
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            CardNumber = cardNumber,
            CinemaIds = cinemaIds ?? []
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
