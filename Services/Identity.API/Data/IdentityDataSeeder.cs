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
        await EnsureRoleAsync(roleManager, Roles.Admin);
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

        // Admin account for exercising the Admin-only endpoints
        await EnsureUserAsync(
            userManager,
            id: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            userName: "admin",
            email: "admin@cinematf.local",
            firstName: "Admin",
            lastName: "CineMATF",
            cardNumber: "340000000000009",
            role: Roles.Admin);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string name)
    {
        if (!await roleManager.RoleExistsAsync(name))
        {
            await roleManager.CreateAsync(new IdentityRole(name));
        }
    }

    private static async Task EnsureUserAsync(UserManager<User> userManager, string id, string userName, string email, string firstName, string lastName, string cardNumber, string role)
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
            CardNumber = cardNumber
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
