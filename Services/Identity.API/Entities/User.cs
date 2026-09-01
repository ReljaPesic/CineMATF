using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CardNumber { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
