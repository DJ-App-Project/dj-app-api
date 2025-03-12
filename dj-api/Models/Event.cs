using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace dj_api.Models
{
    [BsonIgnoreExtraElements]
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        //ko bo baza done treba dodat
    }
}
