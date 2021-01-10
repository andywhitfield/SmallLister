namespace SmallLister.Web.Model.Profile
{
    public class ExternalApiClientModel
    {
        public ExternalApiClientModel(int apiClientId, string displayName, string appKey, string redirectUri, bool isEnabled)
        {
            ApiClientId = apiClientId;
            DisplayName = displayName;
            AppKey = appKey;
            RedirectUri = redirectUri;
            IsEnabled = isEnabled;
        }

        public int ApiClientId { get; }
        public string DisplayName { get; }
        public string AppKey { get; }
        public string RedirectUri { get; }
        public bool IsEnabled { get; }
    }
}