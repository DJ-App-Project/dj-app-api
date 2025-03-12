using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/GuestUsers")]

// API za delo z gosti
public class GuestUserController : ControllerBase
{
    private readonly GuestUserRepository _guestUserRepository;

    public GuestUserController(GuestUserRepository guestUserRepository) // konstruktor
    {
        _guestUserRepository = guestUserRepository;
    }

    [HttpGet]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> GetAllUsers()// GET api za vse goste
    {
        var users = await _guestUserRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> GetUserById(string id)// GET api za enega gosta po ID
    {
        var user = await _guestUserRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "ApiKeyPolicy")]
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
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> DeleteUser(string id)// DELETE api za brisanje gosta po ID

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
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> UpdateUser(string id, GuestUser newUser)// PUT api za posodabljanje gosta po ID
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
