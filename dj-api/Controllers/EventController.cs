using dj_api.ApiModels.Event.Post;
using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/event")]

// API za delo z dogodki/eventi + QR kodo
public class EventController : ControllerBase
{
    private readonly EventRepository _eventsRepository;
    private readonly UserRepository _userRepository;

    public EventController(EventRepository eventRepository, UserRepository userRepository) // konstruktor za EventController
    {
        _eventsRepository = eventRepository;
        _userRepository = userRepository;
    }

    [SwaggerOperation(Summary = "DEPRECATED: Get all events (use paginated version)")]
    [HttpGet("/events-old")]
    [Authorize]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _eventsRepository.GetAllEventsAsync(); // pridobi vse dogodke
        if (events.Count > 0 && events != null) // če so dogodki najdeni
        {
            return Ok(events); // vrni dogodke
        }
        return NotFound(); // če ni dogodkov, vrni NotFound
    }
    [SwaggerOperation(Summary = "Get paginated events data")]
    [HttpGet]// GET api za vse dogodke
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

    [HttpGet("{id}")]// GET api za en dogodek po ID
    [Authorize]
    public async Task<IActionResult> GetEventById(string id)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(id); // pridobi dogodek po ID
        if (eventy == null)
            return NotFound(); // če dogodek ni najden, vrni NotFound

        return Ok(eventy); // če je dogodek najden, vrni dogodek
    }


    [HttpPost]// POST api za kreiranje novega dogodka
    [Authorize]
    public async Task<IActionResult> CreateEvent(Event eventy)
    {
        if (eventy == null)
        {
            return BadRequest("Event data missing"); // če ni podatkov o dogodku, vrni BadRequest
        }

        try
        {
            await _eventsRepository.CreateEventAsync(eventy); // ustvari nov dogodek
            return CreatedAtAction("GetEventById", new { id = eventy.Id }, eventy); // vrni ustvarjeni dogodek
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // če je prišlo do napake, vrni BadRequest z sporočilom napake
        }
    }

    [HttpDelete("{id}")]// DELETE api za brisanje dogodka po ID
    [Authorize]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        try
        {
            await _eventsRepository.DeleteEventAsync(id);
            return NoContent(); // če je dogodek uspešno izbrisan, vrni NoContent
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // če je prišlo do napake, vrni BadRequest z sporočilom napake
        }
    }


    [HttpPut("{id}")]// PUT api za posodabljanje dogodka po ID
    [Authorize]
    public async Task<IActionResult> UpdateEvent(string id, Event newEvent)
    {
        if (id != newEvent.Id)
            return BadRequest(); // če ID dogodka ni enak ID-ju novega dogodka, vrni BadRequest
        var existingEvent = await _eventsRepository.GetEventByIdAsync(id);
        if (existingEvent == null)
            return NotFound(); // če dogodek ne obstaja, vrni NotFound

        await _eventsRepository.UpdateEventAsync(id, newEvent);
        return NoContent(); // če je dogodek uspešno posodobljen, vrni NoContent
    }


    [HttpGet("{id}/qrcode")]// GET api za png QR kodo dogodka po ID
    [Authorize]
    public async Task<IActionResult> GetEventQrCode(string id)
    {
        var QRImg = await _eventsRepository.GenerateQRCode(id);

        if (QRImg != null && QRImg.Length > 0) // če je QR koda generirana
        {
            return File(QRImg, "image/png"); // vrni QR kodo v obliki slike
        }
        return NotFound(); // če QR koda ni generirana, vrni NotFound
    }

    [HttpGet("{id}/music-details")] //GET API Za pridobitev muzik v DJ-jovem playlistu in pregledom s voti. 
    [Authorize]
    public async Task<IActionResult> GetMusicDetailsForEvent(string id)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(id);

        if (eventy == null) //ce id ni v bazi
            return NotFound($"Event with ID {id} not found.");

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

            return Ok();
        }
        catch (Exception ex) 
        {
            return BadRequest("Error when addding music");
        }
    }

    /*[HttpPost("/AddVoteToMusicEvent")]
    [Authorize]
    public async Task<IActionResult> AddVoteToMusicEvent()
    {

    }*/
}
