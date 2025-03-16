using dj_api.Models;
using dj_api.ApiModels.Event.Get;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dj_api.Repositories
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(string eventId);
        Task<List<Event>> FindEvents(string UserId);
        Task DeleteEventAsync(string eventId);
        Task CreateEventAsync(Event newEvent);
        Task<bool> UpdateEventAsync(string eventId, Event updatedEvent);
        Task<bool> AddSongToEventAsync(string eventId, Song song, string userId);
        Task<List<Song>> GetSilimarSongsToEvent(string eventId);
        Task<List<EventGet>> GetPaginatedEventsAsync(int page, int pageSize);
        Task<byte[]> GenerateQRCode(string EventId);
    }
}