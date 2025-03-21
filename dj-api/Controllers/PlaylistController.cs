﻿using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace dj_api.Controllers
{
    [ApiController]
    [Route("api/playlist")]
    public class PlaylistController : Controller
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistController(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        [SwaggerOperation(Summary = "DEPRECATED: Get all playlists (use paginated version) zaenkrat se baza ni spemna za to")]
        [HttpGet("playlist-old")] // 
        //[Authorize] 
        public async Task<IActionResult> GetAllPlayList()
        {
            var playlists = await _playlistRepository.GetAllPlaylist();
            return Ok(playlists);
        }
    }
}
