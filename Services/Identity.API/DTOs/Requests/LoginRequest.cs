using System.ComponentModel.DataAnnotations;

namespace Identity.API.DTOs.Requests;

// Body of POST /api/v1/Auth/Login.

public record LoginRequest(
    [Required] string UserName,
    [Required] string Password
);
