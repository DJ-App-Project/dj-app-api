using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using static dj_api.Models.Event;

namespace dj_api.ApiModels.Event.Get
{
    public class EventGet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }
        [BsonElement("DJID")]
        public string DJId { get; set; }
        [BsonElement("QRCode")]
        public string QRCodeText { get; set; }
       
        [BsonElement("EnableUserRecommendation")]
        public bool EnableUserRecommendation { get; set; } = false;
    }
}
