namespace dj_api.ApiModels.Event.Post
{
    public class SetEnableUserRecommendationPost
    {
        public string ObjectId { get; set; } = null!;

        public bool EnableUserRecommendation { get; set; } = false;

    }
}
