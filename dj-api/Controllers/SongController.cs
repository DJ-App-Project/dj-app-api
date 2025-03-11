using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/songs")]
public class SongController : ControllerBase
{
    private readonly SongRepository _songRepository;

    public SongController(SongRepository songRepository)
    {
        _songRepository = songRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSongs()
    {
        var songs = await _songRepository.GetAllSongsAsync();
        return Ok(songs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSongById(string id)
    {
        var song = await _songRepository.GetSongByIdAsync(id);
        if (song == null)
            return NotFound();

        return Ok(song);
    }

}
