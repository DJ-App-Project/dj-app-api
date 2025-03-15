using Microsoft.AspNetCore.Mvc;
using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace dj_api.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : Controller
    {
        private readonly ISongPlayRepository _songPlayRepository;

        public AnalyticsController(ISongPlayRepository songPlayRepository)
        {
            _songPlayRepository = songPlayRepository;
        }

        [SwaggerOperation(Summary = "Record a song play event")]
        [HttpPost("play/{songId}")]
        [Authorize]
        public async Task<IActionResult> RecordSongPlay(string songId)
        {
            await _songPlayRepository.RecordPlayAsync(songId);
            return Ok(new { Message = "Song play recorded successfully." });
        }

        [SwaggerOperation(Summary = "Get the most played song globally")]
        [HttpGet("most-played")]
        [Authorize]
        public async Task<IActionResult> GetMostPlayedSong()
        {
            var mostPlayedSong = await _songPlayRepository.GetMostPLayedSongAsync();
            if (mostPlayedSong == null)
                return NotFound(new { Message = "No song plays recorded yet." });
            return Ok(new
            {
                mostPlayedSong.ObjectId,
                mostPlayedSong.Name,
                mostPlayedSong.Artist,
                mostPlayedSong.Genre,
                mostPlayedSong.AddedAt,
            });
        }
    }
}
