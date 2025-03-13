﻿using dj_api.ApiModels.Event.Get;
using dj_api.ApiModels.Event.Post;
using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

[ApiController]
[Route("api/event")]

public class EventController : ControllerBase
{
    private readonly EventRepository _eventsRepository;
    private readonly UserRepository _userRepository;
    private readonly SongRepository _songRepository;

    public EventController(EventRepository eventRepository, UserRepository userRepository, SongRepository songRepository) // konstruktor za EventController
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
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetEventById(string id)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(id);
        if (eventy == null)
            return NotFound();

        return Ok(eventy);
    }

    [SwaggerOperation(Summary = "Delete Event by Id")]
    [HttpDelete("{EventId}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(string EventId)
    {
        if(EventId == null)
        {
            return BadRequest("Invalid data");
        }
        var Event = _eventsRepository.GetEventByIdAsync(EventId);
        if(Event == null)
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

        if (QRImg != null && QRImg.Length > 0) // če je QR koda generirana
        {
            return File(QRImg, "image/png"); // vrni QR kodo v obliki slike
        }
        return NotFound(); // če QR koda ni generirana, vrni NotFound
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
        if(EventId == null)
        {
            return BadRequest("Error in data");
        }
        Event Event = await _eventsRepository.GetEventByIdAsync(EventId);
        var musicList = Event?.MusicConfig?.MusicPlaylist?
        .Select(m => new MusicGet
        {
            ObjectId = m.ObjectId,
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
        if(data == null)
        {
            return BadRequest("Data missing");
        }
        var user = _userRepository.GetUserByIdAsync(data.DjId);
        if(user == null)
        {
            return BadRequest("DjId is not a user");
        }
        try
        {
            Event newEvent = new Event
            {
                DJId = data.DjId,
                QRCodeText = data.QRCodeText,
                MusicConfig = new Event.MusicConfigClass()
            };
            await _eventsRepository.CreateEventAsync(newEvent);
            return Ok(new
            {
                Message = "Event created successfully.",
                ObjectId = newEvent.ObjectId
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
        if(data == null)
        {
            return BadRequest("Request error");
        }
        var Event = await _eventsRepository.GetEventByIdAsync(data.ObjectId);
        if(Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        try
        {
            Event.MusicConfig.EnableUserRecommendation = data.EnableUserRecommendation;
            await _eventsRepository.UpdateEventAsync(Event.ObjectId, Event);

            return Ok();    
        }
        catch (Exception ex) {
            return BadRequest("Error when SetEnableUserRecommendation");
        };
        


    }

    [SwaggerOperation(Summary = "Add a specific music to event")]
    [HttpPost("/AddMusicToEvent")]
    [Authorize]
    public async Task<IActionResult> AddMusicToEvent(AddMusicToEventModelPost data)
    {
        if(data==null)
        {
            return BadRequest("data invalid");
        }
        var Event = await _eventsRepository.GetEventByIdAsync(data.EvendId);
        if (Event == null)
        {
            return BadRequest("Event doesn't exist");
        }
        var User = await _userRepository.GetUserByIdAsync(data.RecommenderID);
        if (User == null)
        {
            return BadRequest("User doesn't exist");
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
                VotersIDs = [],
                IsUserRecommendation = data.IsUserRecommendation,
                RecommenderID = data.RecommenderID,

            };
            Event.MusicConfig.MusicPlaylist.Add(newMusic);
            await _eventsRepository.UpdateEventAsync(Event.ObjectId, Event);

        // Extract relevant details and sort by votes (descending)
        var musicDetails = eventy.MusicConfig?.MusicPlaylist?
            .OrderByDescending(m => m.Votes) //po votih padajoce
            .Select(m => new
            {
                MusicName = m.MusicName,
                Visible = m.Visible,
                Votes = m.Votes,
                IsUserRecommendation = m.IsUserRecommendation,
                RecommenderID = m.RecommenderID
            }).ToList();

        return Ok(musicDetails);
    }

}
