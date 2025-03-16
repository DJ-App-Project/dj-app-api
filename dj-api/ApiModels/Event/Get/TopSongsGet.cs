namespace dj_api.ApiModels.Event.Get
{
    public class TopSongsGet
    {
        public string Genre { get; set; } = string.Empty;
        public int TotalVotes { get; set; }
        public int SongCount { get; set; }
    }
}
