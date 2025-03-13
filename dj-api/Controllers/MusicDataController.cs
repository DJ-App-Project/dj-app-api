using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/music-data")]
public class MusicDataController : ControllerBase
{
    private readonly MusicDataRepository _musicDataRepository;

    public MusicDataController(MusicDataRepository musicDataRepository)
    {
        _musicDataRepository = musicDataRepository;
    }

    [SwaggerOperation(Summary = "DEPRECATED: Get all music (use paginated version)")]
    [HttpGet("/music-old")]
    [Authorize]
    public async Task<IActionResult> GetAllMusicData()
    {
        var musicData = await _musicDataRepository.GetAllMusicDataAsync();
        return Ok(musicData);
    }
    [SwaggerOperation(Summary = "Get paginated music data")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllMusicPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and pageSize must be greater than 0.");
        }

        var paginatedResult = await _musicDataRepository.GetPaginatedMusicDataAsync(page, pageSize);

        if (paginatedResult.Count == 0)
        {
            return NotFound("No music found.");
        }

        return Ok(paginatedResult);
    }
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetMusicDataById(string id)
    {
        var musicData = await _musicDataRepository.GetMusicDataByIdAsync(id);
        if (musicData == null)
            return NotFound();

        return Ok(musicData);
    }
}
