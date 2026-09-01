using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs.Requests;

// Body of POST /api/v1/Auth/RegisterUser and .../RegisterAdmin.
public record RegisterRequest
{
    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }

    // Becomes User.UserName and is what the user logs in with.
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not a valid e-mail address")]
    public string Email { get; set; }

    // Loyalty / payment card. Maps to User.CardNumber by name.
    // Swap [Required] for [CreditCard] if Luhn validation is wanted.
    [Required(ErrorMessage = "CardNumber is required")]
    public string CardNumber { get; set; }

    // Optional - IdentityUser already has a PhoneNumber column, so AutoMapper
    // copies this across by name with no extra configuration.
    [Phone(ErrorMessage = "PhoneNumber is not a valid phone number")]
    public string? PhoneNumber { get; set; }
}
