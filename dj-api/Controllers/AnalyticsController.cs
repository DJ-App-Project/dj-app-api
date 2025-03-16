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
        private readonly EventRepository _eventsRepository;
        private readonly SongPlayRepository _songPlayRepository;

        public AnalyticsController(EventRepository eventRepository, SongPlayRepository songPlayRepository)
        {
            _eventsRepository = eventRepository;
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

        /// <summary>
        /// Get top songs across all events.
        /// </summary>
        /// <param name="topN">Number of top songs to retrieve. Default is 10.</param>
        /// <returns>A list of top songs.</returns>
        [SwaggerOperation(Summary = "Get top songs across all events")]
        [HttpGet("analytics/top-songs")]
        [Authorize]
        public async Task<IActionResult> GetTopSongs([FromQuery] int topN = 10)
        {
            var topSongs = await _eventsRepository.GetTopSongsAsync(topN);
            return Ok(topSongs);
        }

        /// <summary>
        /// Get genre popularity across all events.
        /// </summary>
        /// <returns>Genre popularity statistics.</returns>
        [SwaggerOperation(Summary = "Get genre popularity across all events")]
        [HttpGet("analytics/genre-popularity")]
        [Authorize]
        public async Task<IActionResult> GetGenrePopularity()
        {
            var genrePopularity = await _eventsRepository.GetGenrePopularityAsync();
            return Ok(genrePopularity);
        }

        /// <summary>
        /// Get user contribution metrics.
        /// </summary>
        /// <returns>User contribution statistics.</returns>
        [SwaggerOperation(Summary = "Get user contribution metrics")]
        [HttpGet("analytics/user-contributions")]
        [Authorize]
        public async Task<IActionResult> GetUserContributions()
        {
            var userContributions = await _eventsRepository.GetUserContributionMetricsAsync();
            return Ok(userContributions);
        }

        /// <summary>
        /// Get performance metrics for all events.
        /// </summary>
        /// <returns>Event performance statistics.</returns>
        [SwaggerOperation(Summary = "Get performance metrics for all events")]
        [HttpGet("analytics/event-performance")]
        [Authorize]
        public async Task<IActionResult> GetEventPerformance()
        {
            var eventPerformance = await _eventsRepository.GetEventPerformanceMetricsAsync();
            return Ok(eventPerformance);
        }
    }
}
