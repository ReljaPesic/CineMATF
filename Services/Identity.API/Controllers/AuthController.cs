using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Identity.API.Controllers.Base;
using Identity.API.DTOs.Requests;
using Identity.API.DTOs.Responses;
using Identity.API.Entities;
using Identity.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.API.Controllers;

// Public authentication surface.

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : RegistrationControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        ILogger<RegistrationControllerBase> logger,
        IMapper mapper,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IAuthService authService)
        : base(logger, mapper, userManager, roleManager)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }
    
    //   POST /api/v1/Auth/RegisterUser
    // Creates a normal account (role "User").
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        return await RegisterNewUserWithRoles(request, new[] { Roles.User });
    }
    
    //   POST /api/v1/Auth/RegisterAdmin
    // Creates an administrator account (role "Admin").
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        return await RegisterNewUserWithRoles(request, new[] { Roles.Admin });
    }
    
    //   POST /api/v1/Auth/Login
    // Exchanges username + password for a fresh access/refresh token pair.
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.ValidateUser(request);
        if (user is null)
        {
            _logger.LogWarning($"{nameof(Login)}: Authentication failed. Wrong username or password.");
            return Unauthorized();
        }

        return Ok(await _authService.CreateAuthResponse(user));
    }
    
    //   POST /api/v1/Auth/Refresh
    // Exchanges a still-valid refresh token for a new access/refresh pair.
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest refreshTokenCredentials)
    {
        var user = await _userManager.FindByNameAsync(refreshTokenCredentials.UserName);
        if (user == null)
        {
            _logger.LogWarning($"{nameof(Refresh)}: Refreshing token failed. Unknown username {refreshTokenCredentials.UserName}.");
            return Forbid();
        }

        var refreshToken = user.RefreshTokens.FirstOrDefault(r => r.Token == refreshTokenCredentials.RefreshToken);
        if (refreshToken == null)
        {
            _logger.LogWarning($"{nameof(Refresh)}: Refreshing token failed. The refresh token is not found.");
            return Unauthorized();
        }

        if (refreshToken.ExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning($"{nameof(Refresh)}: Refreshing token failed. The refresh token is not valid.");
            return Unauthorized();
        }

        // Rotate: the used token is single-use - delete it (persisted), then
        // issue a fresh pair.
        await _authService.RemoveRefreshToken(user, refreshToken.Token);

        return Ok(await _authService.CreateAuthResponse(user));
    }
    
    //   POST /api/v1/Auth/Logout
    // Invalidates one refresh token
    [Authorize]
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest refreshTokenCredentials)
    {
        var user = await _userManager.FindByNameAsync(refreshTokenCredentials.UserName);
        if (user == null)
        {
            _logger.LogWarning($"{nameof(Logout)}: Logout failed. Unknown username {refreshTokenCredentials.UserName}.");
            return Forbid();
        }

        await _authService.RemoveRefreshToken(user, refreshTokenCredentials.RefreshToken);

        return Accepted();
    }
}
