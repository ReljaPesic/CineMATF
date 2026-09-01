using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs.Requests;

// Body of PUT /api/v1/User/{username}. Only the editable profile fields - the
// username (login key) and password (own flow) are deliberately not here.
public record UpdateUserRequest
{
    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not a valid e-mail address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "CardNumber is required")]
    public string CardNumber { get; set; }

    [Phone(ErrorMessage = "PhoneNumber is not a valid phone number")]
    public string? PhoneNumber { get; set; }
}
