namespace dj_api.ApiModels.Event.Get
{
    public class AwardGet
    {
        public string AwardName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SongId { get; set; }
        public string? MusicName { get; set; }
        public string? MusicArtist { get; set; }
        public int? Votes { get; set; }
    }
}
