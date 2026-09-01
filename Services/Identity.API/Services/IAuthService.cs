using System.Threading.Tasks;
using Identity.API.DTOs.Requests;
using Identity.API.DTOs.Responses;
using Identity.API.Entities;

namespace Identity.API.Services;

public interface IAuthService
{
    Task<User?> ValidateUser(LoginRequest request);
    Task<AuthResponse> CreateAuthResponse(User user);
    Task RemoveRefreshToken(User user, string refreshToken);
}
