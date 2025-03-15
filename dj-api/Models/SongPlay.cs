using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace dj_api.Models
{
    public class SongPlay
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SongId { get; set; }

        [BsonElement("PlayedAt")]
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    }
}
