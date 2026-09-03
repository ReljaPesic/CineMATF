namespace Identity.API.DTOs;

// Minimal projection for service-to-service lookups (e.g. Reservation.API sending ticket emails).
// Deliberately excludes fields like CardNumber that UserDetails exposes to the user themselves.
public class UserContactInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
