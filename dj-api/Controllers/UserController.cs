﻿using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
	private readonly UserRepository _userRepository;

	public UserController(UserRepository userRepository)
	{
		_userRepository = userRepository;
	}
  
  [HttpGet]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

	[HttpGet("{id}")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> GetUserById(string id)
	{
		var user = await _userRepository.GetUserByIdAsync(id);
		if (user == null)
			return NotFound();

		return Ok(user);
	}

	[HttpPost]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> CreateUser(User user)
    {
        await _userRepository.CreateUserAsync(user);
        return CreatedAtAction("GetUserById", new { id = user.Id }, user);
    }

	[HttpDelete("{id}")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> DeleteUser(string id)

    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();
        await _userRepository.DeleteUserAsync(id);
        return NoContent();
    }

	[HttpPut("{id}")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> UpdateUser(string id, User newUser)
    {
        if (Convert.ToInt32(id) != newUser.Id)
            return BadRequest();
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser == null)
            return NotFound();
        await _userRepository.UpdateUserAsync(id, newUser);
        return NoContent();
       
    }   
}
