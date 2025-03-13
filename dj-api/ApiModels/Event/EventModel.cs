using MongoDB.Bson.Serialization.Attributes;
using static dj_api.Models.Event;

namespace dj_api.ApiModels.Event
{
    public class EventModel
    {
        public string DJId { get; set; } = null!;

        public string QRCodeText { get; set; } = null!;

        public MusicConfigClassModel MusicConfig { get; set; } = null!;
    }
}
