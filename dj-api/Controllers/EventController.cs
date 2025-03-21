using dj_api.ApiModels.Event.Get;
using dj_api.ApiModels.Event.Post;
using dj_api.ApiModels.Event.Put;
using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

[ApiController]
[Route("api/event")]
public class EventController : ControllerBase
{
    private readonly IEventRepository _eventsRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISongRepository _songRepository;

    public EventController(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        ISongRepository songRepository)
    {
        _eventsRepository = eventRepository;
        _userRepository = userRepository;
        _songRepository = songRepository;
    }

    [SwaggerOperation(Summary = "DEPRECATED: Get all events (use paginated version)")]
    [HttpGet("/events-old")]
    [Authorize]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _eventsRepository.GetAllEventsAsync();
        if (events.Count > 0 && events != null)
        {
            return Ok(events);
        }
        return NotFound();
    }

    [SwaggerOperation(Summary = "Get event based on EventId")]
    [HttpGet("{EventId}")]
    [Authorize]
    public async Task<IActionResult> GetEventById(string EventId)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(EventId);
        if (eventy == null)
            return NotFound();

        return Ok(eventy);
    }

    [HttpGet("EventsFromUser")]
    [Authorize]
    public async Task<IActionResult> EventFromUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        if (userId == null)
        {
            return BadRequest("Error in UserId");
        }
        try
        {
            List<Event> Events = await _eventsRepository.FindEvents(userId);
            List<EventGet> filteredEvents = Events.Select(e => new EventGet
            {
                ObjectId = e.ObjectId,
                DJId = e.DJId,
                QRCodeText = e.QRCodeText,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Active = e.Active,
                EnableUserRecommendation = e.MusicConfig.EnableUserRecommendation,
            }).ToList();
            return Ok(filteredEvents);
        }
        catch 
        {
            return BadRequest("Error when creating events");
        }
    }

    [SwaggerOperation(Summary = "Delete Event by Id")]
    [HttpDelete("{EventId}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(string EventId)
    {
        if (EventId == null)
        {
            return BadRequest("Invalid data");
        }
        var Event = _eventsRepository.GetEventByIdAsync(EventId);
        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        try
        {
            await _eventsRepository.DeleteEventAsync(EventId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [SwaggerOperation(Summary = "Create QR code based on eventid")]
    [HttpGet("{EventId}/qrcode")]
    [Authorize]
    public async Task<IActionResult> GetEventQrCode(string EventId)
    {
        var QRImg = await _eventsRepository.GenerateQRCode(EventId);

        if (QRImg != null && QRImg.Length > 0)
        {
            return File(QRImg, "image/png");
        }
        return NotFound();
    }

    [SwaggerOperation(Summary = "Get paginated Events (only events)")]
    [HttpGet("/AllEvents")]
    [Authorize]
    public async Task<IActionResult> GetAllEventsPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and pageSize must be greater than 0.");
        }

        var paginatedResult = await _eventsRepository.GetPaginatedEventsAsync(page, pageSize);

        if (paginatedResult.Count == 0)
        {
            return NotFound("No events found.");
        }

        return Ok(paginatedResult);
    }

    [SwaggerOperation(Summary = "Get all Music on specific event by eventid")]
    [HttpGet("/Music/{EventId}")]
    [Authorize]
    public async Task<IActionResult> GetMusicOfEvent(string EventId)
    {
        if (EventId == null)
        {
            return BadRequest("Error in data");
        }
        Event Event = await _eventsRepository.GetEventByIdAsync(EventId);
        var musicList = Event?.MusicConfig?.MusicPlaylist?
            .Select(m => new MusicGet
            {
                MusicName = m.MusicName,
                MusicArtist = m.MusicArtist,
                MusicGenre = m.MusicGenre,
                Votes = m.Votes,
                Visible = m.Visible,
                IsUserRecommendation = m.IsUserRecommendation,
            }).ToList() ?? new List<MusicGet>();

        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        return Ok(musicList);
    }

    [SwaggerOperation(Summary = "Create event")]
    [HttpPost("/CreateEvent")]
    [Authorize]
    public async Task<IActionResult> CreateEvent(CreateEventPost data)
    {
        if (data == null)
        {
            return BadRequest("Data missing");
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        try
        {
            Event newEvent = new Event
            {
                DJId = userId,
                Name = data.Name,
                Description = data.Description,
                Date = data.Date,
                Location = data.Location,
                Active = data.Active,
                MusicConfig = new Event.MusicConfigClass()
            };
            await _eventsRepository.CreateEventAsync(newEvent);
            return Ok(new CreateEventResponse
            {
                Message = "Event created successfully.",
                EventId = newEvent.ObjectId
            });
        }
        catch (Exception ex)
        {
            return BadRequest("Error when creating Event");
        }
    }

    [SwaggerOperation(Summary = "Enable or disable UserRecommendation")]
    [HttpPost("/SetEnableUserRecommendation")]
    [Authorize]
    public async Task<IActionResult> SetEnableUserRecommendation(SetEnableUserRecommendationPost data)
    {
        if (data == null)
        {
            return BadRequest("Request error");
        }
        var Event = await _eventsRepository.GetEventByIdAsync(data.EventId);
        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        try
        {
            Event.MusicConfig.EnableUserRecommendation = data.EnableUserRecommendation;
            await _eventsRepository.UpdateEventAsync(Event.ObjectId, Event);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest("Error when SetEnableUserRecommendation");
        }
    }

    [SwaggerOperation(Summary = "Add a specific music to event if it doesnt exist its added to song")]
    [HttpPost("/AddMusicToEvent")]
    [Authorize]
    public async Task<IActionResult> AddMusicToEvent(AddMusicToEventModelPost data)
    {
        bool whoaddedSong = true;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        if (data == null)
        {
            return BadRequest("data invalid");
        }
        var Event = await _eventsRepository.GetEventByIdAsync(data.EvendId);
        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        if (Event.MusicConfig.EnableUserRecommendation == false || Event.DJId != userId)
        {
            return BadRequest("Only Dj can add songs");
        }
        if (userId == Event.DJId)
        {
            whoaddedSong = false;
        }
        else
        {
            whoaddedSong = true;
        }
        var songTitle = _songRepository.FindSongByTitleAsync(data.MusicName);
        var songArtist = _songRepository.FindSongsByArtistAsync(data.MusicArtist);
        if (songArtist == null || songTitle == null)
        {
            Song newSong = new Song
            {
                Name = data.MusicName,
                Genre = data.MusicGenre,
                Artist = data.MusicArtist,
                AddedAt = DateTime.UtcNow,
            };
            await _songRepository.CreateSongAsync(newSong);
        }

        try
        {
            MusicData newMusic = new MusicData()
            {
                MusicName = data.MusicName,
                MusicArtist = data.MusicArtist,
                MusicGenre = data.MusicGenre,
                Visible = data.Visible,
                Votes = 0,
                VotersIDs = new List<string>(),
                IsUserRecommendation = whoaddedSong,
                RecommenderID = userId,
            };
            Event.MusicConfig.MusicPlaylist.Add(newMusic);
            await _eventsRepository.UpdateEventAsync(Event.ObjectId, Event);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest("Error");
        }
    }

    [HttpPost("{eventId}/vote-unlisted/{songId}")]
    [Authorize]
    public async Task<IActionResult> VoteForUnlistedSong(string eventId, string songId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        var song = await _songRepository.GetSongByIdAsync(songId);
        if (song == null)
        {
            return NotFound(new { message = "Song not found in Songs collection." });
        }
        var success = await _eventsRepository.AddSongToEventAsync(eventId, song, userId);
        if (!success)
        {
            return BadRequest(new { message = "Song is already in the event playlist." });
        }
        return Ok(new { message = "Song added to the event and first vote recorded!" });
    }

    [SwaggerOperation(Summary = "Vote on a song in Event - 1 user can only vote for 1 song")]
    [HttpPost("VoteForEventSong/{eventId}")]
    public async Task<IActionResult> VoteForEventSong(VoteForEventSongPost data, string eventId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        var eventy = await _eventsRepository.GetEventByIdAsync(eventId);
        if (eventy == null)
        {
            return BadRequest(new { message = "Event doesn't exist" });
        }
        if (eventy.Active == false)
        {
            return BadRequest("Event didn't start yet");
        }
        try
        {
            var existingSong = eventy.MusicConfig?.MusicPlaylist?
                .FirstOrDefault(m => string.Equals(m.MusicName, data.MusicName, StringComparison.OrdinalIgnoreCase) &&
                                     string.Equals(m.MusicArtist, data.MusicArtist, StringComparison.OrdinalIgnoreCase));
            if (existingSong == null)
            {
                return BadRequest(new { message = "Song doesn't exist in event" });
            }
            bool hasUserVoted = eventy.MusicConfig.MusicPlaylist
                .Any(m => m.VotersIDs.Contains(userId));
            if (hasUserVoted)
            {
                return BadRequest(new { message = "User has already voted for a song in this event" });
            }
            existingSong.VotersIDs.Add(userId);
            existingSong.Votes += 1;
            await _eventsRepository.UpdateEventAsync(eventId, eventy);
            return Ok(new { message = "Vote successfully recorded!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error when adding vote to song", error = ex.Message });
        }
    }

    [HttpPut("ChangeQRCodeText/{EventId}")]
    [Authorize]
    public async Task<IActionResult> ChangeQRCodeText(ChangeQRCodeTextPut data, string EventId)
    {
        if (data == null)
        {
            return BadRequest("error in data");
        }
        var Event = await _eventsRepository.GetEventByIdAsync(EventId);
        if (Event == null)
        {
            return BadRequest("Event doesnt exist");
        }
        try
        {
            Event.QRCodeText = data.ChangeQRCodeText;
            await _eventsRepository.UpdateEventAsync(EventId, Event);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest("Error when updating ChangeQRCodeText");
        }
    }

    [HttpPut("RemoveVoteFromEvent/{EventId}")]
    [Authorize]
    public async Task<IActionResult> RemoveVoteFromEvent(string EventId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        var Event = await _eventsRepository.GetEventByIdAsync(EventId);
        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        bool hasUserVoted = Event.MusicConfig?.MusicPlaylist?
            .Any(m => m.VotersIDs.Contains(userId)) ?? false;
        if (!hasUserVoted)
        {
            return BadRequest("User didn't vote");
        }
        try
        {
            var votedSong = Event.MusicConfig?.MusicPlaylist?
                .FirstOrDefault(m => m.VotersIDs?.Contains(userId) == true);
            if (votedSong == null)
            {
                return BadRequest("Song doesn't exist");
            }
            votedSong.Votes -= 1;
            votedSong.VotersIDs.Remove(userId);
            await _eventsRepository.UpdateEventAsync(EventId, Event);
            return Ok(new { message = "Vote removed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred", details = ex.Message });
        }
    }

    [HttpPost("RemoveMusicFromEvent/{EventId}")]
    [Authorize]
    public async Task<IActionResult> RemoveMusicFromEvent(string EventId, RemoveMusicFromEventPut data)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        var Event = await _eventsRepository.GetEventByIdAsync(EventId);
        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        var musicToRemove = Event.MusicConfig?.MusicPlaylist?
            .FirstOrDefault(m => m.MusicName == data.MusicName && m.MusicArtist == data.MusicArtist);
        if (musicToRemove == null)
        {
            return BadRequest("Music not found in event playlist");
        }
        try
        {
            Event.MusicConfig.MusicPlaylist.Remove(musicToRemove);
            await _eventsRepository.UpdateEventAsync(EventId, Event);
            return Ok(new { message = "Music removed successfully from the event." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while removing music.", details = ex.Message });
        }
    }

    [SwaggerOperation(Summary = "Get similar songs to existing playlist based on EventId")]
    [HttpGet("{EventId}/SimilarSongs")]
    [Authorize]
    public async Task<IActionResult> GetSimilarSongs(string EventId)
    {
        List<Song>? similarSongs = new List<Song>();
        similarSongs = await _eventsRepository.GetSilimarSongsToEvent(EventId);
        if (similarSongs == null)
            return NotFound();
        return Ok(similarSongs);
    }

    [HttpGet("{eventId}/leaderboard")]
    [Authorize]
    public async Task<IActionResult> GetEventLeaderboard(string eventId)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(eventId);
        if (eventy == null)
        {
            return NotFound(new { message = "Event not found." });
        }
        var leaderboard = eventy.MusicConfig?.MusicPlaylist?
            .OrderByDescending(m => m.Votes)
            .Select((m, index) => new
            {
                Rank = index + 1,
                MusicName = m.MusicName,
                Artist = m.MusicArtist,
                Votes = m.Votes,
                IsUserRecommendation = m.IsUserRecommendation
            }).ToList();
        return Ok(leaderboard);
    }

    /// <summary>
    /// Vote to skip a specific song in an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="songId">The song ID.</param>
    /// <returns>Action result indicating the outcome</returns>
    [SwaggerOperation(Summary = "Vote to skip a specific song in an event.")]
    [HttpPost("{eventId}/skip/{songId}")]
    [Authorize]
    public async Task<IActionResult> VoteToSkipSong(string eventId, string songId)
    {
        var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(UserId))
        {
            return Unauthorized(new { message = "User authentication required." });
        }
        var currentEvent = await _eventsRepository.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound(new { message = "Event not found." });
        }
        if (!currentEvent.Active)
        {
            return BadRequest(new { message = "Event is not active." });
        }
        var song = currentEvent.MusicConfig?.MusicPlaylist?
            .FirstOrDefault(m => m.ObjectId == songId);
        if (song == null)
        {
            return NotFound(new { message = "Song not found in event playlist." });
        }
        if (song.VotersIDs.Contains(UserId))
        {
            return BadRequest(new { message = "User has already voted to skip this song." });
        }
        song.Votes += 1;
        song.VotersIDs.Add(UserId);
        int totalActiveUsers = await _userRepository.GetTotalActiveUsersAsync();
        int requiredVotes = (int)Math.Ceiling(totalActiveUsers / 2.0) + 1;
        bool shouldSkip = song.Votes >= requiredVotes;
        if (shouldSkip)
        {
            await SkipSongAsync(currentEvent, song);
            return Ok(new
            {
                Message = "Vote recorded. Skip threshold reached. The song has been skipped.",
                CurrentVotes = song.Votes,
                VotesNeeded = requiredVotes,
                ShouldSkip = shouldSkip
            });
        }
        await _eventsRepository.UpdateEventAsync(eventId, currentEvent);
        return Ok(new
        {
            Message = "Vote recorded.",
            CurrentVotes = song.Votes,
            VotesNeeded = requiredVotes,
            ShouldSkip = shouldSkip
        });
    }

    /// <summary>
    /// Get the current skip vote status for a specific song in an event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="songId">The ID of the song.</param>
    /// <returns>Current skip vote status.</returns>
    [SwaggerOperation(Summary = "Get the current skip vote status for a specific song in an event.")]
    [HttpGet("{eventId}/skip/status/{songId}")]
    [Authorize]
    public async Task<IActionResult> GetSkipVoteStatus(string eventId, string songId)
    {
        var currentEvent = await _eventsRepository.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound(new { message = "Event not found." });
        }
        var song = currentEvent.MusicConfig?.MusicPlaylist?
            .FirstOrDefault(m => m.ObjectId == songId);
        if (song == null)
        {
            return NotFound(new { message = "Song not found in event playlist." });
        }
        int totalActiveUsers = await _userRepository.GetTotalActiveUsersAsync();
        int requiredVotes = (int)Math.Ceiling(totalActiveUsers / 2.0) + 1;
        bool shouldSkip = song.Votes >= requiredVotes;
        return Ok(new
        {
            CurrentVotes = song.Votes,
            VotesNeeded = requiredVotes,
            ShouldSkip = shouldSkip
        });
    }

    /// <summary>
    /// Skips the specified song in the event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    /// <param name="song">The song to skip.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SkipSongAsync(Event currentEvent, MusicData song)
    {
        currentEvent.MusicConfig.MusicPlaylist.Remove(song);
        await _eventsRepository.UpdateEventAsync(currentEvent.ObjectId, currentEvent);
    }

    /// <summary>
    /// Get awards for a specific event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A list of awards.</returns>
    [SwaggerOperation(Summary = "Get awards for a specific event")]
    [HttpGet("{eventId}/awards")]
    [Authorize]
    public async Task<IActionResult> GetEventAwards(string eventId)
    {
        var currentEvent = await _eventsRepository.GetEventByIdAsync(eventId);
        if (currentEvent == null)
        {
            return NotFound(new { message = "Event not found." });
        }
        var awards = await _eventsRepository.GenerateAwardsAsync(currentEvent);
        if (awards == null || awards.Count == 0)
        {
            return Ok(new { message = "No awards available for this event." });
        }
        return Ok(awards);
    }
}
