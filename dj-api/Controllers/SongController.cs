using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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


    [HttpGet("{id}")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> GetSongById(string id)
    {
        var song = await _songRepository.GetSongByIdAsync(id);
        if (song == null)
            return NotFound();

        return Ok(song);
    }

}
