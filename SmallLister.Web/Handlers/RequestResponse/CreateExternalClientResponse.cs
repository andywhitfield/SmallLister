namespace SmallLister.Web.Handlers.RequestResponse
{
    public class CreateExternalClientResponse
    {
        public CreateExternalClientResponse(string displayName, string redirectUri, string appKey, string appSecret)
        {
            DisplayName = displayName;
            RedirectUri = redirectUri;
            AppKey = appKey;
            AppSecret = appSecret;
        }
        public string DisplayName { get; }
        public string RedirectUri { get; }
        public string AppKey { get; }
        public string AppSecret { get; }
    }
}