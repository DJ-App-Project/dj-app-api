using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]

// API za delo z uporabniki
public class UserController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public UserController(UserRepository userRepository) // konstruktor za UserController
    {
        _userRepository = userRepository;
    }
  
  [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()// GET api za vse uporabnike
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(string id) // GET api za enega uporabnika po ID
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(); // če uporabnik ni najden, vrni NotFound
        return Ok(user);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateUser(User user) // POST api za kreiranje novega uporabnika
    {
        if (user == null)
            return BadRequest("User data missing"); // če ni podatkov o uporabniku, vrni BadRequest

        try
        {
            await _userRepository.CreateUserAsync(user);
            return CreatedAtAction("GetUserById", new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // če je prišlo do napake, vrni BadRequest z sporočilom napake
        }
    }


	[HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string id) // DELETE api za brisanje uporabnika po ID
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(); // če uporabnik ni najden, vrni NotFound

        try
        {
            await _userRepository.DeleteUserAsync(id);
            return NoContent(); // če je uporabnik uspešno izbrisan, vrni NoContent
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // če je prišlo do napake, vrni BadRequest z sporočilom napake
        }
    }

	[HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string id, User newUser) // PUT api za posodabljanje uporabnika po ID
    {
        if (Convert.ToInt32(id) != newUser.Id)
            return BadRequest(); // če ID uporabnika ni enak ID-ju novega uporabnika, vrni BadRequest
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser == null)
            return NotFound(); // če uporabnik ni najden, vrni NotFound
        await _userRepository.UpdateUserAsync(id, newUser);
        return NoContent(); // če je uporabnik uspešno posodobljen, vrni NoContent
    }
}
