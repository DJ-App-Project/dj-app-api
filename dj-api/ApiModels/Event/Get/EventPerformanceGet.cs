namespace dj_api.ApiModels.Event.Get
{
    public class EventPerformanceGet
    {
        public string EventId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int TotalSongs { get; set; }
        public int TotalVotes { get; set; }
        public double AverageVotesPerSong { get; set; }
    }
}
