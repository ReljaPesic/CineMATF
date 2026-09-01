using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Identity.API.DTOs;
using Identity.API.DTOs.Requests;
using Identity.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Identity.API.Controllers.Base;

public abstract class RegistrationControllerBase : ControllerBase
{
    protected readonly ILogger<RegistrationControllerBase> _logger;
    protected readonly IMapper _mapper;
    protected readonly UserManager<User> _userManager;
    protected readonly RoleManager<IdentityRole> _roleManager;

    protected RegistrationControllerBase(ILogger<RegistrationControllerBase> logger, IMapper mapper, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _mapper = mapper;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected async Task<IActionResult> RegisterNewUserWithRoles(RegisterRequest request, IEnumerable<string> roles)
    {
        var user = _mapper.Map<User>(request);
        // Password is in plaintext and the _userManager handles the crypting
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        _logger.LogInformation($"Successfully added new user: {user.UserName}");

        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
                _logger.LogInformation($"Added role {role} to user {user.UserName}");
            }
            else
            {
                _logger.LogWarning($"Role {role} does not exist - skipped");
            }
        }

        return StatusCode(StatusCodes.Status201Created);
    }
}
