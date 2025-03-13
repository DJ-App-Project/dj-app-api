using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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


    [SwaggerOperation(Summary = "DEPRECATED: Get all users (use paginated version)")]
    [HttpGet("/user-old")]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()// GET api za vse uporabnike
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }
    [SwaggerOperation(Summary = "Get paginated user data")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUserPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and pageSize must be greater than 0.");
        }

        var paginatedResult = await _userRepository.GetPaginatedUserAsync(page, pageSize);

        if (paginatedResult.Count == 0)
        {
            return NotFound("No Users found.");
        }

        return Ok(paginatedResult);
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
