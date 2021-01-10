using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Api
{
    public class ApiAuthorizeModel : BaseViewModel
    {
        public string ApplicationName { get; }
        public string AppKey { get; }
        public string RedirectUri { get; }
        public ApiAuthorizeModel(HttpContext context, string applicationName, string appKey, string redirectUri) : base(context)
        {
            ApplicationName = applicationName;
            AppKey = appKey;
            RedirectUri = redirectUri;
        }
    }
}