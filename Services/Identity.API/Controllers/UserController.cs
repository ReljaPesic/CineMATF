using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Identity.API.DTOs;
using Identity.API.DTOs.Requests;
using Identity.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public UserController(UserManager<User> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }
    
    //   GET /api/v1/User
    // Admin-only: list every user
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDetails>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDetails>>> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<UserDetails>>(users));
    }

    //   GET /api/v1/User/{username}
    // An Admin can view anyone; a plain User can view only their own account
    // (same rule as PUT below).
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(UserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetails>> GetUser(string username)
    {
        if (!User.IsInRole(Roles.Admin) &&
            !string.Equals(User.Identity?.Name, username, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<UserDetails>(user));
    }

    //   PUT /api/v1/User/{username}
    // Update a user's profile fields.
    // An Admin can edit anyone 
    // User can only edit their own account 
    [Authorize(Roles = Roles.Admin + "," + Roles.User)]
    [HttpPut("{username}")]
    [ProducesResponseType(typeof(UserDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetails>> UpdateUser(string username, [FromBody] UpdateUserRequest request)
    {
        if (!User.IsInRole(Roles.Admin) &&
            !string.Equals(User.Identity?.Name, username, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user is null)
        {
            return NotFound();
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.CardNumber = request.CardNumber;
        user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        return Ok(_mapper.Map<UserDetails>(user));
    }

    //   DELETE /api/v1/User
    // Admin-only: permanently delete a user.
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{username}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(string username)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        return NoContent();
    }
}