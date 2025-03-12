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
            await _eventsCollection.InsertOneAsync(eventy);
        }

        public async Task DeleteEventAsync(string id)
        {
            await _eventsCollection.DeleteOneAsync(e => e.Id == id);
        }

        public async Task UpdateEventAsync(string id, Event eventy)
        {
            await _eventsCollection.ReplaceOneAsync(e => e.Id == id, eventy);
        }

        //QR Code generacija iz teksta v bazi
        public async Task<Byte[]> GenerateQRCode(string EventId)
        {
            Byte[] qrCodeImg = null;
            Event eventy;

            eventy = await _eventsCollection.Find(e => e.Id == EventId).FirstOrDefaultAsync();
            if (eventy == null)
                return null;

            using (var qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(eventy.QRCodeText, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                qrCodeImg = qrCode.GetGraphic(20);
            }

            return qrCodeImg;
        }
    }
}
