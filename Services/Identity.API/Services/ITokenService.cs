using System.Collections.Generic;
using Identity.API.Entities;

namespace Identity.API.Services;

// The token factory
public interface ITokenService
{
   
    string CreateAccessToken(User user, IEnumerable<string> roles);

    RefreshToken GenerateRefreshToken();
}
