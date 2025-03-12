using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dj_api.Models
{
    [BsonIgnoreExtraElements]
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("ID")]
        public string Id { get; set; }

        [BsonElement("DJID")]
        public string DJId { get; set; }

        [BsonElement("QRCode")]
        public string QRCodeText { get; set; } 

        [BsonElement("MusicConfig")]
        public MusicConfigClass MusicConfig { get; set; }

        public class MusicConfigClass
        {
            [BsonElement("MusicPlaylist")]
            public List<MusicData> MusicPlaylist { get; set; }

            [BsonElement("EnableUserRecommendation")]
            public bool EnableUserRecommendation { get; set; }
        }

    }
}
