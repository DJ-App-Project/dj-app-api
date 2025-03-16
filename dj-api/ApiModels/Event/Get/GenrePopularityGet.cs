using MongoDB.Bson.Serialization.Attributes;

namespace dj_api.ApiModels.Event.Get
{
    public class GenrePopularityGet
    {
        public string Genre { get; set; } = string.Empty;
        public int TotalVotes { get; set; }
        public int SongCount { get; set; }
    }
}
