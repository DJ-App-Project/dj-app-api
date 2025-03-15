using dj_api.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace dj_api.ApiModels.Event.Get.ReturnEvent
{
    public class MusicConfigGet
    {
        [BsonElement("MusicPlaylist")]
        public List<MusicDataGet> MusicPlaylist { get; set; } = new List<MusicDataGet>()!;

        [BsonElement("EnableUserRecommendation")]
        public bool EnableUserRecommendation { get; set; } = false;
    }
}
