namespace dj_api.ApiModels.Event.Post
{
    public class CreateEventPost
    {
        public string DjId { get; set; } = null!;

        public string QRCodeText { get; set; } = null!;
    }
}
