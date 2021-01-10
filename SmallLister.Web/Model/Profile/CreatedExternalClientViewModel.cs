using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Profile
{
    public class CreatedExternalClientViewModel : BaseViewModel
    {
        public CreatedExternalClientViewModel(HttpContext context, string displayName, string redirectUri, string appKey, string appSecret) : base(context)
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