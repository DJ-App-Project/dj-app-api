using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dj_api.Models
{
    [BsonIgnoreExtraElements]
    public class MusicData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("MusicName")]
        public string MusicName { get; set; }

        [BsonElement("Visible")]
        public bool Visible { get; set; }

        [BsonElement("Votes")]
        public int Votes { get; set; }

        [BsonElement("VotersIDs")]
        public string VotersIDs { get; set; }

        [BsonElement("IsUserRecommendation")]
        public bool IsUserRecommendation { get; set; }

        [BsonElement("RecommenderID")]
        public string RecommenderID { get; set; }
    }
}
