namespace dj_api.ApiModels.Event.Get
{
    public class UserContributionGet
    {
        // The ID of the user (or recommender).
        public string UserId { get; set; } = string.Empty;

        // Count of recommendations made by the user.
        public int Recommendations { get; set; }

        // Total votes received on the songs the user recommended.
        public int Votes { get; set; }
    }
}
