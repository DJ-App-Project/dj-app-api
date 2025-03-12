using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace dj_api.Models
{
    [BsonIgnoreExtraElements]
    public class GuestUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectId { get; set; }

        [BsonElement("ID")]
        public int Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }

        //v bazi jih je vecina NaN?
        //[BsonElement("DeletedAt")]
        //public DateTime DeletedAt { get; set; }

    }
}