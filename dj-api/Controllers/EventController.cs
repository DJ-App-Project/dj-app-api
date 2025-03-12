using dj_api.Models;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/event")]
public class EventController : ControllerBase
{
    private readonly EventRepository _eventsRepository;

    public EventController(EventRepository eventRepository)
    {
        _eventsRepository = eventRepository;
    }
    [HttpGet]
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

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetEventById(string id)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(id);
        if (eventy == null)
            return NotFound();

        return Ok(eventy);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateEvent(Event eventy)
    {
        await _eventsRepository.CreateEventAsync(eventy);
        return CreatedAtAction("GetEventById", new { id = eventy.Id }, eventy);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var eventy = await _eventsRepository.GetEventByIdAsync(id);
        if (eventy == null)
            return NotFound();
        await _eventsRepository.DeleteEventAsync(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateEvent(string id, Event newEvent)
    {
        if (id != newEvent.Id)
            return BadRequest();
        var existingEvent = await _eventsRepository.GetEventByIdAsync(id);
        if (existingEvent == null)
            return NotFound();
        await _eventsRepository.UpdateEventAsync(id, newEvent);
        return NoContent();
    }

    [HttpGet("{id}/qrcode")]
    [Authorize]
    public async Task<IActionResult> GetEventQrCode(string id)
    {
        var QRImg = await _eventsRepository.GenerateQRCode(id);

        if (QRImg != null && QRImg.Length > 0)
        {
            return File(QRImg, "image/png");
        }
        return NotFound();
    }

}
