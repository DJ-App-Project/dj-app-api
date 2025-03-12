using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/music-data")]
public class MusicDataController : ControllerBase
{
    private readonly MusicDataRepository _musicDataRepository;

    public MusicDataController(MusicDataRepository musicDataRepository)
    {
        _musicDataRepository = musicDataRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMusicData()
    {
        var musicData = await _musicDataRepository.GetAllMusicDataAsync();
        return Ok(musicData);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMusicDataById(string id)
    {
        var musicData = await _musicDataRepository.GetMusicDataByIdAsync(id);
        if (musicData == null)
            return NotFound();

        return Ok(musicData);
    }
}
