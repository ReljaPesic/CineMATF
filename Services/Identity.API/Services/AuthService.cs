using Identity.API.Data;
using Identity.API.DTOs.Requests;
using Identity.API.DTOs.Responses;
using Identity.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IdentityContext _dbContext;

    public AuthService(UserManager<User> userManager, ITokenService tokenService, IdentityContext dbContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    // Username/password check
    public async Task<User?> ValidateUser(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        // Password is in plaintext and the _userManager hashes it
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return null;
        }

        return user;
    }

    
    public async Task<AuthResponse> CreateAuthResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.CreateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshTokens.Add(refreshToken);
        _dbContext.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    // Logout / token rotation: forget one refresh token. No-op if it isn't
    // there - the end state (token gone) is the same either way.
    public async Task RemoveRefreshToken(User user, string refreshToken)
    {
        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token is null)
        {
            return;
        }

        user.RefreshTokens.Remove(token);
        _dbContext.Remove(token);
        await _dbContext.SaveChangesAsync();
    }
}
