using dj_api.Data;
using dj_api.Models;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using QRCoder;

namespace dj_api.Repositories
{
    public class EventRepository
    {
        private readonly IMongoCollection<Event> _eventsCollection;

        public EventRepository(MongoDbContext dbContext)
        {
            _eventsCollection = dbContext.GetCollection<Event>("DJEvent");
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _eventsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(string id)
        {
            return await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateEventAsync(Event eventy)
        {
            var existing = await _eventsCollection.Find(e => e.Id == eventy.Id).FirstOrDefaultAsync();
            if (existing != null)
                throw new Exception($"Event s {eventy.Id} že obstaja"); // če event že obstaja, vrni Exception

            await _eventsCollection.InsertOneAsync(eventy); // ustvari nov event
        }

        public async Task DeleteEventAsync(string id) // brisanje eventa po ID
        {
            var existing = await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (existing == null)
                throw new Exception($"Event s {id} ne obstaja"); // če event ne obstaja, vrni Exception

            await _eventsCollection.DeleteOneAsync(e => e.Id == id);
        }

        public async Task UpdateEventAsync(string id, Event eventy)
        {
            await _eventsCollection.ReplaceOneAsync(e => e.Id == id, eventy); // posodobi event
        }

        //QR Code generacija iz teksta v bazi
        public async Task<Byte[]> GenerateQRCode(string EventId)
        {
            Byte[] qrCodeImg = null!;
            Event eventy;

            eventy = await _eventsCollection.Find(e => e.Id == EventId).FirstOrDefaultAsync(); // poišči event po ID
            if (eventy == null)
                throw new Exception($"Event s {EventId} ne obstaja"); // če event ne obstaja, vrni Exception

            using (var qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(eventy.QRCodeText, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                qrCodeImg = qrCode.GetGraphic(20); // generiraj QR kodo iz teksta
            }

            return qrCodeImg; // vrni QR kodo
        }
        public async Task<List<Event>> GetPaginatedEventsAsync(int page, int pageSize)
        {
            var totalCount = await _eventsCollection.CountDocumentsAsync(_ => true); 

            var events = await _eventsCollection
                .Find(_ => true) 
                .Skip((page - 1) * pageSize) 
                .Limit(pageSize) 
                .ToListAsync();

            return events;
        }
    }
}
