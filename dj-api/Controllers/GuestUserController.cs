using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/GuestUsers")]
public class GuestUserController : ControllerBase
{
    private readonly GuestUserRepository _guestUserRepository;

    public GuestUserController(GuestUserRepository guestUserRepository)
    {
        _guestUserRepository = guestUserRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _guestUserRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _guestUserRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(GuestUser user)
    {
        await _guestUserRepository.CreateUserAsync(user);
        return CreatedAtAction("GetUserById", new { id = user.Id }, user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)

    {
        var user = await _guestUserRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();
        await _guestUserRepository.DeleteUserAsync(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, GuestUser newUser)
    {
        if (Convert.ToInt32(id) != newUser.Id)
            return BadRequest();
        var existingUser = await _guestUserRepository.GetUserByIdAsync(id);
        if (existingUser == null)
            return NotFound();
        await _guestUserRepository.UpdateUserAsync(id, newUser);
        return NoContent();

    }
}
