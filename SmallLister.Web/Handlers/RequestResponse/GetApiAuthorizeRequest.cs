using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetApiAuthorizeRequest : IRequest<GetApiAuthorizeResponse>
    {
        public string AppKey { get; }
        public string RedirectUri { get; }
        public GetApiAuthorizeRequest(string appKey, string redirectUri)
        {
            AppKey = appKey;
            RedirectUri = redirectUri;
        }
    }
}