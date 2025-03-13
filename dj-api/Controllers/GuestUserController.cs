using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/GuestUsers")]
public class GuestUserController : ControllerBase
{
    private readonly GuestUserRepository _guestUserRepository;

    public GuestUserController(GuestUserRepository guestUserRepository) // konstruktor
    {
        _guestUserRepository = guestUserRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _guestUserRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _guestUserRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateUser(GuestUser user)// POST api za kreiranje novega gosta
    {
        if (user == null)
            return BadRequest("User data missing"); // če ni podatkov o gostu, vrni BadRequest

        try
        {
            await _guestUserRepository.CreateUserAsync(user);
            return CreatedAtAction("GetUserById", new { id = user.Id }, user); // vrni ustvarjenega gosta
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // če je prišlo do napake, vrni BadRequest z sporočilom napake
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string id)

    {
        var user = await _guestUserRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(); // če gost ni najden, vrni NotFound

        try
        {
            await _guestUserRepository.DeleteUserAsync(id);
            return NoContent(); // če je gost uspešno izbrisan, vrni NoContent
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // če je prišlo do napake, vrni BadRequest z sporočilom napake
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string id, GuestUser newUser)
    {
        if (Convert.ToInt32(id) != newUser.Id)
            return BadRequest(); // če ID ni enak ID-ju gosta, vrni BadRequest
        var existingUser = await _guestUserRepository.GetUserByIdAsync(id);
        if (existingUser == null)
            return NotFound(); // če gost ni najden, vrni NotFound
        await _guestUserRepository.UpdateUserAsync(id, newUser);
        return NoContent(); // če je gost uspešno posodobljen, vrni NoContent
    }
}
