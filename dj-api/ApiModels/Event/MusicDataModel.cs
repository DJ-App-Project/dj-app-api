using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace dj_api.ApiModels.Event
{
    public class MusicDataModel
    {

        public string MusicName { get; set; } = null!;

        public bool Visible { get; set; }

        public int Votes { get; set; }

        public List<string> VotersIDs { get; set; } = null!;

        public bool IsUserRecommendation { get; set; }

        public string RecommenderID { get; set; } = null!;
    }
}
