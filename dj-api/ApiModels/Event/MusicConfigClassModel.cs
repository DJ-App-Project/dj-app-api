using dj_api.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace dj_api.ApiModels.Event
{
    public class MusicConfigClassModel
    {
        public List<MusicDataModel> MusicPlaylist { get; set; } = null!;


        public bool EnableUserRecommendation { get; set; }
    }
}
